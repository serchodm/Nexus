using Microsoft.EntityFrameworkCore;
using Nexus.Domain.Entities;
using Nexus.Domain.Repositories;

namespace Nexus.Infrastructure.Persistence.Repositories;

public sealed class CategorizationRuleRepository : ICategorizationRuleRepository
{
    private readonly ApplicationDbContext _context;

    public CategorizationRuleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CategorizationRule?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.CategorizationRules
            .Include(r => r.TargetCategory)
            .Include(r => r.TargetSubcategory)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<List<CategorizationRule>> GetActiveRulesByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.CategorizationRules
            .AsNoTracking()
            .Include(r => r.TargetCategory)
            .Include(r => r.TargetSubcategory)
            .Where(r => r.UserId == userId && r.IsActive)
            .OrderByDescending(r => r.Priority)
            .ToListAsync(ct);
    }

    public async Task AddAsync(CategorizationRule rule, CancellationToken ct = default)
    {
        await _context.CategorizationRules.AddAsync(rule, ct);
    }

    public void Update(CategorizationRule rule)
    {
        _context.CategorizationRules.Update(rule);
    }

    public void Delete(CategorizationRule rule)
    {
        _context.CategorizationRules.Remove(rule);
    }
}
