using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Memory;
using Nexus.Application.Statements.DTOs;
using Nexus.Application.Statements.Models;
using Nexus.Application.Statements.Parsers;
using Nexus.Domain.Enums;
using Nexus.Domain.Repositories;

namespace Nexus.Application.Statements.Commands.PreviewStatement;

public class PreviewStatementCommandHandler
{
    private readonly IBankAccountRepository _accountRepository;
    private readonly IStatementImportRepository _importRepository;
    private readonly StatementParserFactory _parserFactory;
    private readonly IMemoryCache _cache;

    public PreviewStatementCommandHandler(
        IBankAccountRepository accountRepository,
        IStatementImportRepository importRepository,
        StatementParserFactory parserFactory,
        IMemoryCache cache)
    {
        _accountRepository = accountRepository;
        _importRepository = importRepository;
        _parserFactory = parserFactory;
        _cache = cache;
    }

    public async Task<PreviewStatementResult> HandleAsync(PreviewStatementCommand cmd, CancellationToken ct = default)
    {
        // 1. Validar cuenta y titularidad
        var account = await _accountRepository.GetByIdAsync(cmd.BankAccountId, ct);
        if (account == null || account.UserId != cmd.UserId)
        {
            return new PreviewStatementResult(false, null, "La cuenta bancaria especificada no existe o no tienes acceso.");
        }

        // 2. Copiar stream a memoria para hashing y lectura múltiple
        using var memoryStream = new MemoryStream();
        await cmd.FileStream.CopyToAsync(memoryStream, ct);
        var fileBytes = memoryStream.ToArray();

        var hashBytes = SHA256.HashData(fileBytes);
        var fileHash = Convert.ToHexString(hashBytes).ToLowerInvariant();

        // 3. RN-03: Comprobar si este fichero ya fue importado
        var alreadyImported = await _importRepository.ExistsHashForAccountAsync(cmd.BankAccountId, fileHash, ct);
        if (alreadyImported)
        {
            return new PreviewStatementResult(false, null, "Este archivo exacto ya fue importado previamente en esta cuenta.");
        }

        // 4. Leer cabecera y resolver parser
        using var readStream = new MemoryStream(fileBytes);
        var headerLine = await StatementTableReader.ReadHeaderLineAsync(readStream, cmd.FileName, ct);
        
        readStream.Position = 0;
        IStatementParser parser;
        try
        {
            parser = _parserFactory.Resolve(headerLine, account.BankType);
        }
        catch (Exception ex)
        {
            return new PreviewStatementResult(false, null, ex.Message);
        }

        // 5. Parsear transacciones
        readStream.Position = 0;
        StatementParseResult parseResult;
        try
        {
            parseResult = await parser.ParseAsync(readStream, cmd.FileName, ct);
        }
        catch (Exception ex)
        {
            return new PreviewStatementResult(false, null, $"Error al parsear el extracto: {ex.Message}");
        }

        // 6. Evaluar salvaguardas y advertencias
        var warnings = new List<string>();

        // RN-09: Discontinuidad de saldo
        if (account.CurrentBalance.HasValue)
        {
            var diff = Math.Abs(account.CurrentBalance.Value - parseResult.StartingBalance);
            if (diff > 0.05m)
            {
                warnings.Add($"BALANCE_DISCONTINUITY: El saldo actual registrado en la cuenta es {account.CurrentBalance.Value:F2} €, pero el extracto inicia con {parseResult.StartingBalance:F2} € (diferencia de {diff:F2} €).");
            }
        }

        // RN-10: Tarjetas no vinculadas
        if (parseResult.DetectedCards.Count > 0 && account.LinkedCardsLast4 != null)
        {
            foreach (var card in parseResult.DetectedCards)
            {
                if (!account.LinkedCardsLast4.Contains(card))
                {
                    warnings.Add($"CARD_MISMATCH: El extracto contiene pagos con la tarjeta terminada en {card}, que no está registrada en esta cuenta.");
                }
            }
        }

        // 7. Guardar en IMemoryCache por 15 minutos con token efímero
        var previewToken = Guid.NewGuid();
        var cachedData = new CachedPreviewData(
            PreviewToken: previewToken,
            BankAccountId: cmd.BankAccountId,
            UserId: cmd.UserId,
            FileName: cmd.FileName,
            FileHashSha256: fileHash,
            ParseResult: parseResult
        );

        _cache.Set(previewToken, cachedData, TimeSpan.FromMinutes(15));

        // 8. Construir DTO de respuesta
        var totalIncome = parseResult.Transactions.Where(t => t.Amount > 0).Sum(t => t.Amount);
        var totalExpense = parseResult.Transactions.Where(t => t.Amount < 0).Sum(t => Math.Abs(t.Amount));

        var previewRows = parseResult.Transactions
            .Take(5)
            .Select(t => new StatementPreviewRowDto(
                OperationDate: t.OperationDate,
                Description: t.Description,
                Amount: t.Amount,
                BalanceAfter: t.BalanceAfter,
                BankCategory: t.BankCategory
            ))
            .ToList();

        var previewDto = new StatementPreviewDto(
            PreviewToken: previewToken,
            DetectedBankType: parseResult.BankType,
            DetectedBankName: parseResult.BankType.ToString(),
            StartingBalance: parseResult.StartingBalance,
            EndingBalance: parseResult.EndingBalance,
            StartDate: parseResult.StartDate,
            EndDate: parseResult.EndDate,
            TotalRecords: parseResult.Transactions.Count,
            TotalIncome: Math.Round(totalIncome, 2),
            TotalExpense: Math.Round(totalExpense, 2),
            Warnings: warnings,
            PreviewRows: previewRows
        );

        return new PreviewStatementResult(true, previewDto);
    }
}
