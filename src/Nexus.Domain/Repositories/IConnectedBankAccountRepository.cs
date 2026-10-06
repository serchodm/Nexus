using Nexus.Domain.Entities;

namespace Nexus.Domain.Repositories;

public interface IConnectedBankAccountRepository
{
    Task<ConnectedBankAccount?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ConnectedBankAccount?> GetByExternalAccountIdAsync(string externalAccountId, CancellationToken ct = default);
    Task<List<ConnectedBankAccount>> GetByBankConnectionIdAsync(Guid bankConnectionId, CancellationToken ct = default);
    Task AddAsync(ConnectedBankAccount account, CancellationToken ct = default);
    void Update(ConnectedBankAccount account);
    void Delete(ConnectedBankAccount account);
}
