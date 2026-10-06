using Microsoft.EntityFrameworkCore;
using Nexus.Domain.Entities;
using Nexus.Domain.Repositories;

namespace Nexus.Infrastructure.Persistence.Repositories;

public sealed class SharedSettlementRepository : ISharedSettlementRepository
{
    private readonly ApplicationDbContext _context;

    public SharedSettlementRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SharedSettlement?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.SharedSettlements
            .Include(s => s.PayerMember)
            .Include(s => s.ReceiverMember)
            .Include(s => s.SettledExpenses)
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<List<SharedSettlement>> GetHistoryBySpaceIdAsync(Guid spaceId, int take = 50, CancellationToken ct = default)
    {
        return await _context.SharedSettlements
            .Include(s => s.PayerMember)
            .Include(s => s.ReceiverMember)
            .Where(s => s.SharedSpaceId == spaceId)
            .OrderByDescending(s => s.SettledAt)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task AddAsync(SharedSettlement settlement, CancellationToken ct = default)
    {
        await _context.SharedSettlements.AddAsync(settlement, ct);
    }
}
