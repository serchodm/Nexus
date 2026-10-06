using Nexus.Domain.Entities;

namespace Nexus.Domain.Repositories;

public interface IBankConnectionRepository
{
    Task<BankConnection?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<BankConnection?> GetBySessionIdAsync(string externalSessionId, CancellationToken ct = default);
    Task<List<BankConnection>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<List<BankConnection>> GetActiveConnectionsAsync(CancellationToken ct = default);
    Task AddAsync(BankConnection connection, CancellationToken ct = default);
    void Update(BankConnection connection);
    void Delete(BankConnection connection);
}
