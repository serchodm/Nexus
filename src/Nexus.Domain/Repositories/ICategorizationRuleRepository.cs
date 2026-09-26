using Nexus.Domain.Entities;

namespace Nexus.Domain.Repositories;

public interface ICategorizationRuleRepository
{
    Task<CategorizationRule?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<CategorizationRule>> GetActiveRulesByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(CategorizationRule rule, CancellationToken ct = default);
    void Update(CategorizationRule rule);
    void Delete(CategorizationRule rule);
}
