using Nexus.Application.Banking.DTOs;
using Nexus.Application.Banking.Services;
using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Nexus.Domain.Enums;
using Nexus.Domain.Interfaces;
using Nexus.Domain.Repositories;

namespace Nexus.Application.Banking.Commands;

public record SyncBankConnectionCommand(
    Guid UserId,
    Guid ConnectionId,
    bool ForceFullSync = false
);

public class SyncBankConnectionCommandHandler
{
    private readonly IBankConnectionRepository _connectionRepository;
    private readonly IConnectedBankAccountRepository _accountRepository;
    private readonly IBankAccountRepository _internalAccountRepository;
    private readonly IBankTransactionRepository _transactionRepository;
    private readonly IStatementImportRepository _importRepository;
    private readonly IEnableBankingClient _enableBankingClient;
    private readonly IUnitOfWork _unitOfWork;

    public SyncBankConnectionCommandHandler(
        IBankConnectionRepository connectionRepository,
        IConnectedBankAccountRepository accountRepository,
        IBankAccountRepository internalAccountRepository,
        IBankTransactionRepository transactionRepository,
        IStatementImportRepository importRepository,
        IEnableBankingClient enableBankingClient,
        IUnitOfWork unitOfWork)
    {
        _connectionRepository = connectionRepository;
        _accountRepository = accountRepository;
        _internalAccountRepository = internalAccountRepository;
        _transactionRepository = transactionRepository;
        _importRepository = importRepository;
        _enableBankingClient = enableBankingClient;
        _unitOfWork = unitOfWork;
    }

    public async Task<SyncResultDto?> HandleAsync(SyncBankConnectionCommand command, CancellationToken ct = default)
    {
        var connection = await _connectionRepository.GetByIdAsync(command.ConnectionId, ct);
        if (connection == null || connection.UserId != command.UserId)
        {
            return null;
        }

        int totalNewTransactions = 0;
        var now = DateTimeOffset.UtcNow;
        var fromDate = command.ForceFullSync || !connection.LastSyncedAt.HasValue
            ? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-60))
            : DateOnly.FromDateTime(connection.LastSyncedAt.Value.DateTime.AddDays(-2));

        foreach (var acc in connection.Accounts)
        {
            // 1. Sincronizar saldos
            var balanceResult = await _enableBankingClient.GetBalancesAsync(acc.ExternalAccountId, ct);
            acc.CurrentBalance = balanceResult.CurrentBalance;
            acc.AvailableBalance = balanceResult.AvailableBalance ?? balanceResult.CurrentBalance;
            acc.BalanceUpdatedAt = now;

            if (acc.InternalBankAccountId.HasValue)
            {
                var internalAcc = await _internalAccountRepository.GetByIdAsync(acc.InternalBankAccountId.Value, ct);
                if (internalAcc != null)
                {
                    internalAcc.CurrentBalance = acc.CurrentBalance;
                    internalAcc.BalanceLastUpdatedAt = now;
                    _internalAccountRepository.Update(internalAcc);
                }
            }

            // 2. Sincronizar transacciones si hay cuenta interna vinculada
            if (acc.InternalBankAccountId.HasValue)
            {
                var rawTxs = await _enableBankingClient.GetTransactionsAsync(acc.ExternalAccountId, fromDate, ct);
                if (rawTxs.Count > 0)
                {
                    var import = new StatementImport
                    {
                        BankAccountId = acc.InternalBankAccountId.Value,
                        FileName = $"AutoSync_{connection.InstitutionCode}_{now:yyyyMMdd_HHmmss}",
                        FileHashSha256 = $"sync_{Guid.NewGuid():N}",
                        DetectedBankType = BankType.Generic,
                        ImportedAt = now,
                        TotalRecords = rawTxs.Count,
                        StartingBalance = acc.CurrentBalance ?? 0m,
                        EndingBalance = acc.CurrentBalance ?? 0m,
                        StartDate = fromDate,
                        EndDate = DateOnly.FromDateTime(DateTime.UtcNow)
                    };
                    await _importRepository.AddAsync(import, ct);

                    var candidates = new List<(RawBankTransaction Raw, string Fingerprint)>();
                    foreach (var raw in rawTxs)
                    {
                        var opDate = DateOnly.FromDateTime(raw.BookingDate);
                        var fp = Domain.Services.TransactionFingerprintGenerator.Generate(
                            acc.InternalBankAccountId.Value,
                            opDate,
                            raw.Amount,
                            raw.Description,
                            null
                        );
                        candidates.Add((raw, fp));
                    }

                    var candidateFps = candidates.Select(c => c.Fingerprint).ToList();
                    var existingFps = await _transactionRepository.GetExistingFingerprintsAsync(
                        acc.InternalBankAccountId.Value, candidateFps, ct);
                    var existingSet = new HashSet<string>(existingFps, StringComparer.OrdinalIgnoreCase);

                    var newEntities = new List<BankTransaction>();
                    foreach (var (raw, fp) in candidates)
                    {
                        if (!existingSet.Contains(fp))
                        {
                            var txEntity = EnableBankingTransactionMapper.MapToDomain(
                                raw,
                                acc.InternalBankAccountId.Value,
                                import.Id,
                                null
                            );
                            newEntities.Add(txEntity);
                            existingSet.Add(fp); // Prevenir duplicados dentro del mismo lote
                        }
                    }

                    if (newEntities.Count > 0)
                    {
                        await _transactionRepository.AddRangeAsync(newEntities, ct);
                        import.ImportedRecords = newEntities.Count;
                        import.SkippedDuplicates = rawTxs.Count - newEntities.Count;
                        totalNewTransactions += newEntities.Count;
                    }
                    else
                    {
                        import.SkippedDuplicates = rawTxs.Count;
                    }
                }
            }
        }

        connection.LastSyncedAt = now;
        _connectionRepository.Update(connection);
        await _unitOfWork.SaveChangesAsync(ct);

        return new SyncResultDto(
            connection.Id,
            connection.Accounts.Count,
            totalNewTransactions,
            now
        );
    }
}
