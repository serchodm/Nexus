using Nexus.Domain.Entities;
using Nexus.Domain.Enums;

namespace Nexus.Domain.Repositories;

public interface IExternalCommerceConnectionRepository
{
    Task<ExternalCommerceConnection?> GetByUserIdAndProviderAsync(Guid userId, CommerceProvider provider, CancellationToken ct = default);
    Task<List<ExternalCommerceConnection>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(ExternalCommerceConnection connection, CancellationToken ct = default);
    void Update(ExternalCommerceConnection connection);
}
