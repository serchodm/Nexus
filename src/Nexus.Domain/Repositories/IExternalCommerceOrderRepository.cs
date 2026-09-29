using Nexus.Domain.Entities;
using Nexus.Domain.Enums;

namespace Nexus.Domain.Repositories;

public interface IExternalCommerceOrderRepository
{
    Task<ExternalCommerceOrder?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ExternalCommerceOrder?> GetByExternalIdAsync(Guid userId, CommerceProvider provider, string externalOrderId, CancellationToken ct = default);
    Task<List<ExternalCommerceOrder>> GetUnreconciledByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<List<ExternalCommerceOrder>> GetOrdersPagedAsync(Guid userId, CommerceProvider? provider, int page, int pageSize, CancellationToken ct = default);
    Task AddAsync(ExternalCommerceOrder order, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<ExternalCommerceOrder> orders, CancellationToken ct = default);
    void Update(ExternalCommerceOrder order);
}
