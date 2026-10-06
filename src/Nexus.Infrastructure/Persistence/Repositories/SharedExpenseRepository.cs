using Microsoft.EntityFrameworkCore;
using Nexus.Domain.Entities;
using Nexus.Domain.Enums;
using Nexus.Domain.Repositories;

namespace Nexus.Infrastructure.Persistence.Repositories;

public sealed class SharedExpenseRepository : ISharedExpenseRepository
{
    private readonly ApplicationDbContext _context;

    public SharedExpenseRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SharedExpenseEntry?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.SharedExpenseEntries
            .Include(e => e.PaidByMember)
            .Include(e => e.BankTransaction)
            .FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public async Task<List<SharedExpenseEntry>> GetPendingBySpaceIdAsync(Guid spaceId, CancellationToken ct = default)
    {
        return await _context.SharedExpenseEntries
            .Include(e => e.PaidByMember)
            .Where(e => e.SharedSpaceId == spaceId && e.Status == SharedExpenseStatus.Pending)
            .OrderBy(e => e.TransactionDate)
            .ToListAsync(ct);
    }

    public async Task<List<SharedExpenseEntry>> GetSettledHistoryBySpaceIdAsync(Guid spaceId, int take = 50, CancellationToken ct = default)
    {
        return await _context.SharedExpenseEntries
            .Include(e => e.PaidByMember)
            .Include(e => e.Settlement)
            .Where(e => e.SharedSpaceId == spaceId && e.Status == SharedExpenseStatus.Settled)
            .OrderByDescending(e => e.TransactionDate)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task AddAsync(SharedExpenseEntry expense, CancellationToken ct = default)
    {
        await _context.SharedExpenseEntries.AddAsync(expense, ct);
    }

    public void Update(SharedExpenseEntry expense)
    {
        _context.SharedExpenseEntries.Update(expense);
    }

    public void UpdateRange(IEnumerable<SharedExpenseEntry> expenses)
    {
        _context.SharedExpenseEntries.UpdateRange(expenses);
    }
}
