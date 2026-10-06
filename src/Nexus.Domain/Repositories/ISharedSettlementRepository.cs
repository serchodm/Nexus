using Nexus.Domain.Entities;

namespace Nexus.Domain.Repositories;

public interface ISharedSettlementRepository
{
    Task<SharedSettlement?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<SharedSettlement>> GetHistoryBySpaceIdAsync(Guid spaceId, int take = 50, CancellationToken ct = default);
    Task AddAsync(SharedSettlement settlement, CancellationToken ct = default);
}
