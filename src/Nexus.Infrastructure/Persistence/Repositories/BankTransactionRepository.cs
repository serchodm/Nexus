using Microsoft.EntityFrameworkCore;
using Nexus.Domain.Entities;
using Nexus.Domain.Repositories;

namespace Nexus.Infrastructure.Persistence.Repositories;

public sealed class BankTransactionRepository : IBankTransactionRepository
{
    private readonly ApplicationDbContext _context;

    public BankTransactionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<string>> GetExistingFingerprintsAsync(Guid accountId, IEnumerable<string> candidateFingerprints, CancellationToken ct = default)
    {
        var candidates = candidateFingerprints.ToList();
        if (candidates.Count == 0) return new List<string>();

        return await _context.BankTransactions
            .Where(t => t.BankAccountId == accountId && candidates.Contains(t.Fingerprint))
            .Select(t => t.Fingerprint)
            .ToListAsync(ct);
    }

    public async Task<BankTransaction?> GetLatestTransactionByAccountIdAsync(Guid accountId, CancellationToken ct = default)
    {
        return await _context.BankTransactions
            .Where(t => t.BankAccountId == accountId)
            .OrderByDescending(t => t.OperationDate)
            .ThenByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }

    public async Task AddRangeAsync(IEnumerable<BankTransaction> transactions, CancellationToken ct = default)
    {
        await _context.BankTransactions.AddRangeAsync(transactions, ct);
    }

    public async Task DeleteByImportIdAsync(Guid importId, CancellationToken ct = default)
    {
        var transactions = await _context.BankTransactions
            .Where(t => t.StatementImportId == importId)
            .ToListAsync(ct);

        _context.BankTransactions.RemoveRange(transactions);
    }

    public async Task<(List<BankTransaction> Items, int TotalCount)> GetByAccountIdAsync(
        Guid accountId, 
        int page = 1, 
        int pageSize = 50, 
        DateOnly? fromDate = null, 
        DateOnly? toDate = null, 
        CancellationToken ct = default)
    {
        var query = _context.BankTransactions
            .AsNoTracking()
            .Where(t => t.BankAccountId == accountId);

        if (fromDate.HasValue)
        {
            query = query.Where(t => t.OperationDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(t => t.OperationDate <= toDate.Value);
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(t => t.OperationDate)
            .ThenByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
