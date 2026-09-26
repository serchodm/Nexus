using Nexus.Domain.Entities;

namespace Nexus.Domain.Repositories;

public interface IBankTransactionRepository
{
    Task<List<string>> GetExistingFingerprintsAsync(Guid accountId, IEnumerable<string> candidateFingerprints, CancellationToken ct = default);
    Task<BankTransaction?> GetLatestTransactionByAccountIdAsync(Guid accountId, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<BankTransaction> transactions, CancellationToken ct = default);
    Task DeleteByImportIdAsync(Guid importId, CancellationToken ct = default);
    Task<(List<BankTransaction> Items, int TotalCount)> GetByAccountIdAsync(
        Guid accountId, 
        int page = 1, 
        int pageSize = 50, 
        DateOnly? fromDate = null, 
        DateOnly? toDate = null, 
        CancellationToken ct = default);
    Task<BankTransaction?> GetByIdWithSplitsAsync(Guid transactionId, CancellationToken ct = default);
    Task<List<BankTransaction>> GetAutoCategorizedOrUncategorizedByUserIdAsync(Guid userId, CancellationToken ct = default);
    void Update(BankTransaction transaction);
    void UpdateRange(IEnumerable<BankTransaction> transactions);
}
