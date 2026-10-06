using Nexus.Domain.Entities;

namespace Nexus.Domain.Repositories;

public interface ISharedSpaceRepository
{
    Task<SharedSpace?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<SharedSpace?> GetByIdWithMembersAsync(Guid id, CancellationToken ct = default);
    Task<SharedSpace?> GetActiveForUserAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(SharedSpace space, CancellationToken ct = default);
    void Update(SharedSpace space);
}
