using Nexus.Domain.Entities;

namespace Nexus.Domain.Repositories;

public interface IBankAccountRepository
{
    Task<BankAccount?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<BankAccount>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<bool> ExistsIbanForUserAsync(Guid userId, string iban, Guid? excludeAccountId = null, CancellationToken ct = default);
    Task AddAsync(BankAccount account, CancellationToken ct = default);
    void Update(BankAccount account);
}
