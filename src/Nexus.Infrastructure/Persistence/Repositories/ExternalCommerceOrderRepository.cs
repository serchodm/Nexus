using Microsoft.EntityFrameworkCore;
using Nexus.Domain.Entities;
using Nexus.Domain.Enums;
using Nexus.Domain.Repositories;

namespace Nexus.Infrastructure.Persistence.Repositories;

public sealed class ExternalCommerceOrderRepository : IExternalCommerceOrderRepository
{
    private readonly ApplicationDbContext _context;

    public ExternalCommerceOrderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ExternalCommerceOrder?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ExternalCommerceOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    public async Task<ExternalCommerceOrder?> GetByExternalIdAsync(
        Guid userId, 
        CommerceProvider provider, 
        string externalOrderId, 
        CancellationToken ct = default)
    {
        return await _context.ExternalCommerceOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.UserId == userId && o.Provider == provider && o.ExternalOrderId == externalOrderId, ct);
    }

    public async Task<List<ExternalCommerceOrder>> GetUnreconciledByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.ExternalCommerceOrders
            .Include(o => o.Items)
            .Where(o => o.UserId == userId && !o.IsFullyReconciled)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(ct);
    }

    public async Task<List<ExternalCommerceOrder>> GetOrdersPagedAsync(
        Guid userId, 
        CommerceProvider? provider, 
        int page, 
        int pageSize, 
        CancellationToken ct = default)
    {
        var query = _context.ExternalCommerceOrders
            .Include(o => o.Items)
            .Where(o => o.UserId == userId);

        if (provider.HasValue)
        {
            query = query.Where(o => o.Provider == provider.Value);
        }

        return await query
            .OrderByDescending(o => o.OrderDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task AddAsync(ExternalCommerceOrder order, CancellationToken ct = default)
    {
        await _context.ExternalCommerceOrders.AddAsync(order, ct);
    }

    public async Task AddRangeAsync(IEnumerable<ExternalCommerceOrder> orders, CancellationToken ct = default)
    {
        await _context.ExternalCommerceOrders.AddRangeAsync(orders, ct);
    }

    public void Update(ExternalCommerceOrder order)
    {
        _context.ExternalCommerceOrders.Update(order);
    }
}
