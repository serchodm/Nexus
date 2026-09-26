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

    public async Task<(List<BankTransaction> Items, int TotalCount)> SearchTransactionsAsync(
        Guid userId,
        Guid? bankAccountId = null,
        Guid? categoryId = null,
        Guid? subcategoryId = null,
        string? searchTerm = null,
        DateOnly? fromDate = null,
        DateOnly? toDate = null,
        bool? isRecurring = null,
        bool? hasSplits = null,
        Nexus.Domain.Enums.ExpenseNature? expenseNature = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken ct = default)
    {
        var query = _context.BankTransactions
            .AsNoTracking()
            .Include(t => t.Category)
            .Include(t => t.Subcategory)
            .Include(t => t.SplitItems)
                .ThenInclude(s => s.Category)
            .Include(t => t.SplitItems)
                .ThenInclude(s => s.Subcategory)
            .Where(t => t.BankAccount.UserId == userId);

        if (bankAccountId.HasValue)
        {
            query = query.Where(t => t.BankAccountId == bankAccountId.Value);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(t => t.CategoryId == categoryId.Value);
        }

        if (subcategoryId.HasValue)
        {
            query = query.Where(t => t.SubcategoryId == subcategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(t => t.Description.ToLower().Contains(term) ||
                                     (t.CleanMerchantName != null && t.CleanMerchantName.ToLower().Contains(term)));
        }

        if (fromDate.HasValue)
        {
            query = query.Where(t => t.OperationDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(t => t.OperationDate <= toDate.Value);
        }

        if (isRecurring.HasValue)
        {
            query = query.Where(t => t.IsRecurring == isRecurring.Value);
        }

        if (hasSplits.HasValue)
        {
            query = query.Where(t => t.HasSplits == hasSplits.Value);
        }

        if (expenseNature.HasValue)
        {
            query = query.Where(t => t.ExpenseNature == expenseNature.Value);
        }

        var totalCount = await query.CountAsync(ct);

        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = pageSize is < 1 or > 200 ? 50 : pageSize;

        var items = await query
            .OrderByDescending(t => t.OperationDate)
            .ThenByDescending(t => t.CreatedAt)
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<BankTransaction?> GetByIdWithSplitsAsync(Guid transactionId, CancellationToken ct = default)
    {
        return await _context.BankTransactions
            .Include(t => t.Category)
            .Include(t => t.Subcategory)
            .Include(t => t.SplitItems)
                .ThenInclude(s => s.Category)
            .Include(t => t.SplitItems)
                .ThenInclude(s => s.Subcategory)
            .FirstOrDefaultAsync(t => t.Id == transactionId, ct);
    }

    public async Task<List<BankTransaction>> GetAutoCategorizedOrUncategorizedByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.BankTransactions
            .Include(t => t.BankAccount)
            .Where(t => t.BankAccount.UserId == userId && !t.IsManual)
            .ToListAsync(ct);
    }

    public void Update(BankTransaction transaction)
    {
        _context.BankTransactions.Update(transaction);
    }

    public void UpdateRange(IEnumerable<BankTransaction> transactions)
    {
        _context.BankTransactions.UpdateRange(transactions);
    }
}
