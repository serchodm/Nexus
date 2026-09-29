using Microsoft.EntityFrameworkCore;
using Nexus.Domain.Entities;
using Nexus.Domain.Enums;
using Nexus.Domain.Repositories;

namespace Nexus.Infrastructure.Persistence.Repositories;

public sealed class TransactionReconciliationRepository : ITransactionReconciliationRepository
{
    private readonly ApplicationDbContext _context;

    public TransactionReconciliationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TransactionReconciliation?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.TransactionReconciliations
            .Include(r => r.BankTransaction)
            .Include(r => r.ExternalCommerceOrder)
                .ThenInclude(o => o.Items)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<List<TransactionReconciliation>> GetPendingByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.TransactionReconciliations
            .Include(r => r.BankTransaction)
            .Include(r => r.ExternalCommerceOrder)
                .ThenInclude(o => o.Items)
            .Where(r => r.UserId == userId && r.Status == ReconciliationStatus.Candidate)
            .OrderByDescending(r => r.ReconciledAt)
            .ToListAsync(ct);
    }

    public async Task<TransactionReconciliation?> GetByTransactionIdAsync(Guid bankTransactionId, CancellationToken ct = default)
    {
        return await _context.TransactionReconciliations
            .Include(r => r.ExternalCommerceOrder)
                .ThenInclude(o => o.Items)
            .FirstOrDefaultAsync(r => r.BankTransactionId == bankTransactionId, ct);
    }

    public async Task AddAsync(TransactionReconciliation reconciliation, CancellationToken ct = default)
    {
        await _context.TransactionReconciliations.AddAsync(reconciliation, ct);
    }

    public void Update(TransactionReconciliation reconciliation)
    {
        _context.TransactionReconciliations.Update(reconciliation);
    }
}
