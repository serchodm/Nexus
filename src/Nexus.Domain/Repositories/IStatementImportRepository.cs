using Nexus.Domain.Entities;

namespace Nexus.Domain.Repositories;

public interface IStatementImportRepository
{
    Task<StatementImport?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsHashForAccountAsync(Guid accountId, string fileHashSha256, CancellationToken ct = default);
    Task<List<StatementImport>> GetByAccountIdAsync(Guid accountId, CancellationToken ct = default);
    Task AddAsync(StatementImport import, CancellationToken ct = default);
    void Remove(StatementImport import);
}
