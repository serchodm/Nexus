using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Nexus.Domain.Repositories;

namespace Nexus.UnitTests.Fakes;

public class FakeUnitOfWork : IUnitOfWork
{
    public int SaveChangesCalls { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCalls++;
        return Task.FromResult(1);
    }
}

public class FakeBankAccountRepository : IBankAccountRepository
{
    public readonly Dictionary<Guid, BankAccount> Accounts = new();

    public Task<BankAccount?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        Accounts.TryGetValue(id, out var acc);
        return Task.FromResult(acc);
    }

    public Task<List<BankAccount>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return Task.FromResult(Accounts.Values.Where(a => a.UserId == userId).ToList());
    }

    public Task<BankAccount?> GetByIbanAsync(string iban, CancellationToken ct = default)
    {
        return Task.FromResult(Accounts.Values.FirstOrDefault(a => a.Iban == iban));
    }

    public Task<bool> ExistsIbanForUserAsync(Guid userId, string iban, Guid? excludeAccountId = null, CancellationToken ct = default)
    {
        var exists = Accounts.Values.Any(a => a.UserId == userId && a.Iban == iban && (!excludeAccountId.HasValue || a.Id != excludeAccountId.Value));
        return Task.FromResult(exists);
    }

    public Task AddAsync(BankAccount account, CancellationToken ct = default)
    {
        Accounts[account.Id] = account;
        return Task.CompletedTask;
    }

    public void Update(BankAccount account)
    {
        Accounts[account.Id] = account;
    }

    public void Delete(BankAccount account)
    {
        Accounts.Remove(account.Id);
    }
}

public class FakeCategoryRepository : ICategoryRepository
{
    public readonly Dictionary<Guid, Category> Categories = new();

    public Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        Categories.TryGetValue(id, out var cat);
        return Task.FromResult(cat);
    }

    public Task<List<Category>> GetAllSystemCategoriesAsync(CancellationToken ct = default)
    {
        return Task.FromResult(Categories.Values.Where(c => c.IsSystem).ToList());
    }

    public Task<List<Category>> GetCategoriesByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return Task.FromResult(Categories.Values.Where(c => c.UserId == null || c.UserId == userId).ToList());
    }

    public Task<List<Category>> GetCategoryTreeAsync(Guid userId, CancellationToken ct = default)
    {
        return Task.FromResult(Categories.Values.Where(c => (c.UserId == null || c.UserId == userId) && c.ParentCategoryId == null).ToList());
    }

    public Task<Dictionary<string, Category>> GetCategoryLookupAsync(Guid userId, CancellationToken ct = default)
    {
        var lookup = new Dictionary<string, Category>(StringComparer.OrdinalIgnoreCase);
        foreach (var c in Categories.Values.Where(c => c.UserId == null || c.UserId == userId))
        {
            lookup[c.NormalizedName] = c;
            lookup[c.Name] = c;
        }
        return Task.FromResult(lookup);
    }

    public Task AddAsync(Category category, CancellationToken ct = default)
    {
        Categories[category.Id] = category;
        return Task.CompletedTask;
    }

    public void Update(Category category)
    {
        Categories[category.Id] = category;
    }

    public void Delete(Category category)
    {
        Categories.Remove(category.Id);
    }
}

public class FakeCategorizationRuleRepository : ICategorizationRuleRepository
{
    public readonly Dictionary<Guid, CategorizationRule> Rules = new();

    public Task<CategorizationRule?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        Rules.TryGetValue(id, out var rule);
        return Task.FromResult(rule);
    }

    public Task<List<CategorizationRule>> GetActiveRulesByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return Task.FromResult(Rules.Values.Where(r => r.UserId == userId && r.IsActive).OrderByDescending(r => r.Priority).ToList());
    }

    public Task AddAsync(CategorizationRule rule, CancellationToken ct = default)
    {
        Rules[rule.Id] = rule;
        return Task.CompletedTask;
    }

    public void Update(CategorizationRule rule)
    {
        Rules[rule.Id] = rule;
    }

    public void Delete(CategorizationRule rule)
    {
        Rules.Remove(rule.Id);
    }
}

public class FakeBankTransactionRepository : IBankTransactionRepository
{
    public readonly Dictionary<Guid, BankTransaction> Transactions = new();

    public Task<List<string>> GetExistingFingerprintsAsync(Guid accountId, IEnumerable<string> candidateFingerprints, CancellationToken ct = default)
    {
        var set = new HashSet<string>(candidateFingerprints);
        return Task.FromResult(Transactions.Values.Where(t => t.BankAccountId == accountId && set.Contains(t.Fingerprint)).Select(t => t.Fingerprint).ToList());
    }

    public Task<BankTransaction?> GetLatestTransactionByAccountIdAsync(Guid accountId, CancellationToken ct = default)
    {
        return Task.FromResult(Transactions.Values.Where(t => t.BankAccountId == accountId).OrderByDescending(t => t.OperationDate).FirstOrDefault());
    }

    public Task AddRangeAsync(IEnumerable<BankTransaction> transactions, CancellationToken ct = default)
    {
        foreach (var t in transactions) Transactions[t.Id] = t;
        return Task.CompletedTask;
    }

    public Task DeleteByImportIdAsync(Guid importId, CancellationToken ct = default)
    {
        var toRemove = Transactions.Values.Where(t => t.StatementImportId == importId).Select(t => t.Id).ToList();
        foreach (var id in toRemove) Transactions.Remove(id);
        return Task.CompletedTask;
    }

    public Task<(List<BankTransaction> Items, int TotalCount)> GetByAccountIdAsync(Guid accountId, int page = 1, int pageSize = 50, DateOnly? fromDate = null, DateOnly? toDate = null, CancellationToken ct = default)
    {
        var q = Transactions.Values.Where(t => t.BankAccountId == accountId);
        return Task.FromResult((q.Skip((page - 1) * pageSize).Take(pageSize).ToList(), q.Count()));
    }

    public Task<(List<BankTransaction> Items, int TotalCount)> SearchTransactionsAsync(Guid userId, Guid? bankAccountId = null, Guid? categoryId = null, Guid? subcategoryId = null, string? searchTerm = null, DateOnly? fromDate = null, DateOnly? toDate = null, bool? isRecurring = null, bool? hasSplits = null, Nexus.Domain.Enums.ExpenseNature? expenseNature = null, int page = 1, int pageSize = 50, CancellationToken ct = default)
    {
        var q = Transactions.Values.AsEnumerable();
        if (bankAccountId.HasValue) q = q.Where(t => t.BankAccountId == bankAccountId.Value);
        if (categoryId.HasValue) q = q.Where(t => t.CategoryId == categoryId.Value);
        return Task.FromResult((q.Skip((page - 1) * pageSize).Take(pageSize).ToList(), q.Count()));
    }

    public Task<BankTransaction?> GetByIdWithSplitsAsync(Guid transactionId, CancellationToken ct = default)
    {
        Transactions.TryGetValue(transactionId, out var tx);
        return Task.FromResult(tx);
    }

    public Task<List<BankTransaction>> GetAutoCategorizedOrUncategorizedByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return Task.FromResult(Transactions.Values.Where(t => !t.IsManual).ToList());
    }

    public Task ReplaceSplitsAsync(Guid transactionId, IEnumerable<TransactionSplitItem> splitItems, CancellationToken ct = default)
    {
        if (Transactions.TryGetValue(transactionId, out var tx))
        {
            tx.SplitItems.Clear();
            foreach (var item in splitItems)
            {
                tx.SplitItems.Add(item);
            }
        }
        return Task.CompletedTask;
    }

    public void Update(BankTransaction transaction)
    {
        Transactions[transaction.Id] = transaction;
    }

    public void UpdateRange(IEnumerable<BankTransaction> transactions)
    {
        foreach (var t in transactions) Transactions[t.Id] = t;
    }
}

public class FakeExternalCommerceOrderRepository : IExternalCommerceOrderRepository
{
    public readonly Dictionary<Guid, ExternalCommerceOrder> Orders = new();

    public Task<ExternalCommerceOrder?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        Orders.TryGetValue(id, out var o);
        return Task.FromResult(o);
    }

    public Task<ExternalCommerceOrder?> GetByExternalIdAsync(Guid userId, Nexus.Domain.Enums.CommerceProvider provider, string externalOrderId, CancellationToken ct = default)
    {
        return Task.FromResult(Orders.Values.FirstOrDefault(o => o.UserId == userId && o.Provider == provider && o.ExternalOrderId == externalOrderId));
    }

    public Task<List<ExternalCommerceOrder>> GetUnreconciledByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return Task.FromResult(Orders.Values.Where(o => o.UserId == userId && !o.IsFullyReconciled).ToList());
    }

    public Task<List<ExternalCommerceOrder>> GetOrdersPagedAsync(Guid userId, Nexus.Domain.Enums.CommerceProvider? provider, int page, int pageSize, CancellationToken ct = default)
    {
        var q = Orders.Values.Where(o => o.UserId == userId);
        if (provider.HasValue) q = q.Where(o => o.Provider == provider.Value);
        return Task.FromResult(q.Skip((page - 1) * pageSize).Take(pageSize).ToList());
    }

    public Task AddAsync(ExternalCommerceOrder order, CancellationToken ct = default)
    {
        Orders[order.Id] = order;
        return Task.CompletedTask;
    }

    public Task AddRangeAsync(IEnumerable<ExternalCommerceOrder> orders, CancellationToken ct = default)
    {
        foreach (var o in orders) Orders[o.Id] = o;
        return Task.CompletedTask;
    }

    public void Update(ExternalCommerceOrder order)
    {
        Orders[order.Id] = order;
    }
}

public class FakeTransactionReconciliationRepository : ITransactionReconciliationRepository
{
    public readonly Dictionary<Guid, TransactionReconciliation> Reconciliations = new();

    public Task<TransactionReconciliation?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        Reconciliations.TryGetValue(id, out var r);
        return Task.FromResult(r);
    }

    public Task<List<TransactionReconciliation>> GetPendingByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return Task.FromResult(Reconciliations.Values.Where(r => r.UserId == userId && r.Status == Nexus.Domain.Enums.ReconciliationStatus.Candidate).ToList());
    }

    public Task<TransactionReconciliation?> GetByTransactionIdAsync(Guid bankTransactionId, CancellationToken ct = default)
    {
        return Task.FromResult(Reconciliations.Values.FirstOrDefault(r => r.BankTransactionId == bankTransactionId));
    }

    public Task AddAsync(TransactionReconciliation reconciliation, CancellationToken ct = default)
    {
        Reconciliations[reconciliation.Id] = reconciliation;
        return Task.CompletedTask;
    }

    public void Update(TransactionReconciliation reconciliation)
    {
        Reconciliations[reconciliation.Id] = reconciliation;
    }
}

public class FakeExternalCommerceConnectionRepository : IExternalCommerceConnectionRepository
{
    public readonly Dictionary<Guid, ExternalCommerceConnection> Connections = new();

    public Task<ExternalCommerceConnection?> GetByUserIdAndProviderAsync(Guid userId, Nexus.Domain.Enums.CommerceProvider provider, CancellationToken ct = default)
    {
        return Task.FromResult(Connections.Values.FirstOrDefault(c => c.UserId == userId && c.Provider == provider));
    }

    public Task<List<ExternalCommerceConnection>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return Task.FromResult(Connections.Values.Where(c => c.UserId == userId).ToList());
    }

    public Task AddAsync(ExternalCommerceConnection connection, CancellationToken ct = default)
    {
        Connections[connection.Id] = connection;
        return Task.CompletedTask;
    }

    public void Update(ExternalCommerceConnection connection)
    {
        Connections[connection.Id] = connection;
    }
}

