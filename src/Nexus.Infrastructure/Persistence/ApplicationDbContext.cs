using Microsoft.EntityFrameworkCore;
using Nexus.Domain.Common;
using Nexus.Domain.Entities;

namespace Nexus.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IUnitOfWork
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<StatementImport> StatementImports => Set<StatementImport>();
    public DbSet<BankTransaction> BankTransactions => Set<BankTransaction>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<CategorizationRule> CategorizationRules => Set<CategorizationRule>();
    public DbSet<TransactionSplitItem> TransactionSplitItems => Set<TransactionSplitItem>();
    public DbSet<ExternalCommerceOrder> ExternalCommerceOrders => Set<ExternalCommerceOrder>();
    public DbSet<ExternalCommerceOrderItem> ExternalCommerceOrderItems => Set<ExternalCommerceOrderItem>();
    public DbSet<TransactionReconciliation> TransactionReconciliations => Set<TransactionReconciliation>();
    public DbSet<ExternalCommerceConnection> ExternalCommerceConnections => Set<ExternalCommerceConnection>();
    public DbSet<SharedSpace> SharedSpaces => Set<SharedSpace>();
    public DbSet<SharedSpaceMember> SharedSpaceMembers => Set<SharedSpaceMember>();
    public DbSet<SharedExpenseEntry> SharedExpenseEntries => Set<SharedExpenseEntry>();
    public DbSet<SharedSettlement> SharedSettlements => Set<SharedSettlement>();
    public DbSet<BankConnection> BankConnections => Set<BankConnection>();
    public DbSet<ConnectedBankAccount> ConnectedBankAccounts => Set<ConnectedBankAccount>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
