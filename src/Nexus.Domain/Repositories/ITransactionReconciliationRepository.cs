using Nexus.Domain.Entities;

namespace Nexus.Domain.Repositories;

public interface ITransactionReconciliationRepository
{
    Task<TransactionReconciliation?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<TransactionReconciliation>> GetPendingByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<TransactionReconciliation?> GetByTransactionIdAsync(Guid bankTransactionId, CancellationToken ct = default);
    Task AddAsync(TransactionReconciliation reconciliation, CancellationToken ct = default);
    void Update(TransactionReconciliation reconciliation);
}
