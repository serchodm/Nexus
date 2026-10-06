using Nexus.Domain.Entities;

namespace Nexus.Domain.Repositories;

public interface ISharedExpenseRepository
{
    Task<SharedExpenseEntry?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<SharedExpenseEntry>> GetPendingBySpaceIdAsync(Guid spaceId, CancellationToken ct = default);
    Task<List<SharedExpenseEntry>> GetSettledHistoryBySpaceIdAsync(Guid spaceId, int take = 50, CancellationToken ct = default);
    Task AddAsync(SharedExpenseEntry expense, CancellationToken ct = default);
    void Update(SharedExpenseEntry expense);
    void UpdateRange(IEnumerable<SharedExpenseEntry> expenses);
}
