using Microsoft.Extensions.Caching.Memory;
using Nexus.Application.Statements.DTOs;
using Nexus.Application.Statements.Models;
using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Nexus.Domain.Repositories;
using Nexus.Domain.Services;

namespace Nexus.Application.Statements.Commands.ConfirmStatementImport;

public class ConfirmStatementImportCommandHandler
{
    private readonly IBankAccountRepository _accountRepository;
    private readonly IStatementImportRepository _importRepository;
    private readonly IBankTransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;

    public ConfirmStatementImportCommandHandler(
        IBankAccountRepository accountRepository,
        IStatementImportRepository importRepository,
        IBankTransactionRepository transactionRepository,
        IUnitOfWork unitOfWork,
        IMemoryCache cache)
    {
        _accountRepository = accountRepository;
        _importRepository = importRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<ConfirmStatementImportResult> HandleAsync(ConfirmStatementImportCommand cmd, CancellationToken ct = default)
    {
        // 1. Obtener datos de la previsualización desde caché
        if (!_cache.TryGetValue(cmd.PreviewToken, out CachedPreviewData? cached) || cached == null)
        {
            return new ConfirmStatementImportResult(false, null, "La sesión de previsualización ha expirado o no es válida. Por favor, sube el archivo de nuevo.");
        }

        // 2. Validar que la cuenta y el usuario coincidan
        if (cached.UserId != cmd.UserId || cached.BankAccountId != cmd.BankAccountId)
        {
            return new ConfirmStatementImportResult(false, null, "La cuenta especificada no coincide con la previsualización.");
        }

        var account = await _accountRepository.GetByIdAsync(cmd.BankAccountId, ct);
        if (account == null || account.UserId != cmd.UserId)
        {
            return new ConfirmStatementImportResult(false, null, "La cuenta bancaria no existe o no tienes acceso.");
        }

        // 3. RN-03: Validar que el archivo no haya sido importado concurrentemente
        var alreadyImported = await _importRepository.ExistsHashForAccountAsync(cmd.BankAccountId, cached.FileHashSha256, ct);
        if (alreadyImported)
        {
            _cache.Remove(cmd.PreviewToken);
            return new ConfirmStatementImportResult(false, null, "Este archivo exacto ya fue importado en esta cuenta.");
        }

        var parseResult = cached.ParseResult;

        // 4. Crear entidad StatementImport
        var import = new StatementImport
        {
            BankAccountId = cmd.BankAccountId,
            FileName = cached.FileName,
            FileHashSha256 = cached.FileHashSha256,
            DetectedBankType = parseResult.BankType,
            ImportedAt = DateTimeOffset.UtcNow,
            TotalRecords = parseResult.Transactions.Count,
            StartingBalance = parseResult.StartingBalance,
            EndingBalance = parseResult.EndingBalance,
            StartDate = parseResult.StartDate,
            EndDate = parseResult.EndDate
        };

        await _importRepository.AddAsync(import, ct);

        // 5. Generar Fingerprints y deduplicar en memoria (RN-06)
        var transactionsWithFingerprint = parseResult.Transactions.Select(tx =>
        {
            var fp = TransactionFingerprintGenerator.Generate(
                cmd.BankAccountId,
                tx.OperationDate,
                tx.Amount,
                tx.Description,
                tx.BalanceAfter
            );
            return (Raw: tx, Fingerprint: fp);
        }).ToList();

        var candidateFingerprints = transactionsWithFingerprint.Select(t => t.Fingerprint).Distinct();
        var existingFingerprints = await _transactionRepository.GetExistingFingerprintsAsync(cmd.BankAccountId, candidateFingerprints, ct);
        var existingSet = new HashSet<string>(existingFingerprints);

        var toInsert = new List<BankTransaction>();
        var seenInBatch = new HashSet<string>();
        var skippedDuplicates = 0;

        foreach (var item in transactionsWithFingerprint)
        {
            if (existingSet.Contains(item.Fingerprint) || seenInBatch.Contains(item.Fingerprint))
            {
                skippedDuplicates++;
                continue;
            }

            seenInBatch.Add(item.Fingerprint);

            toInsert.Add(new BankTransaction
            {
                BankAccountId = cmd.BankAccountId,
                StatementImportId = import.Id,
                OperationDate = item.Raw.OperationDate,
                ValueDate = item.Raw.ValueDate,
                Description = item.Raw.Description,
                Amount = item.Raw.Amount,
                Currency = item.Raw.Currency,
                BalanceAfter = item.Raw.BalanceAfter,
                BankCategory = item.Raw.BankCategory,
                BankSubcategory = item.Raw.BankSubcategory,
                BankMovementType = item.Raw.BankMovementType,
                Observations = item.Raw.Observations,
                RawDataJson = item.Raw.RawDataJson,
                Fingerprint = item.Fingerprint
            });
        }

        import.ImportedRecords = toInsert.Count;
        import.SkippedDuplicates = skippedDuplicates;

        if (toInsert.Count > 0)
        {
            await _transactionRepository.AddRangeAsync(toInsert, ct);
        }

        // 6. Actualizar CurrentBalance en BankAccount si la importación es más reciente
        if (!account.BalanceLastUpdatedAt.HasValue || parseResult.EndDate >= DateOnly.FromDateTime(account.BalanceLastUpdatedAt.Value.DateTime))
        {
            account.CurrentBalance = parseResult.EndingBalance;
            account.BalanceLastUpdatedAt = parseResult.EndDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            _accountRepository.Update(account);
        }

        // 7. Persistir en una sola transacción unitaria
        await _unitOfWork.SaveChangesAsync(ct);

        // 8. Limpiar caché
        _cache.Remove(cmd.PreviewToken);

        var summary = new ImportSummaryDto(
            ImportId: import.Id,
            BankAccountId: cmd.BankAccountId,
            FileName: import.FileName,
            TotalRecords: import.TotalRecords,
            ImportedRecords: import.ImportedRecords,
            SkippedDuplicates: import.SkippedDuplicates,
            FinalAccountBalance: account.CurrentBalance ?? parseResult.EndingBalance
        );

        return new ConfirmStatementImportResult(true, summary);
    }
}
