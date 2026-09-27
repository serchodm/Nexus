using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Nexus.Application.Statements.Commands.ConfirmStatementImport;
using Nexus.Application.Statements.DTOs;
using Nexus.Application.Statements.Models;
using Nexus.Application.Statements.Parsers;
using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Nexus.Domain.Enums;
using Nexus.Domain.Repositories;
using Nexus.UnitTests.Fakes;
using Xunit;

namespace Nexus.UnitTests;

public class ConfirmStatementImportCommandHandlerTests
{
    private readonly FakeBankAccountRepository _accountRepo;
    private readonly FakeStatementImportRepository _importRepo;
    private readonly FakeBankTransactionRepository _txRepo;
    private readonly FakeCategoryRepository _catRepo;
    private readonly FakeCategorizationRuleRepository _ruleRepo;
    private readonly FakeUnitOfWork _uow;
    private readonly FakeMemoryCache _cache;
    private readonly ConfirmStatementImportCommandHandler _handler;

    public ConfirmStatementImportCommandHandlerTests()
    {
        _accountRepo = new FakeBankAccountRepository();
        _importRepo = new FakeStatementImportRepository();
        _txRepo = new FakeBankTransactionRepository();
        _catRepo = new FakeCategoryRepository();
        _ruleRepo = new FakeCategorizationRuleRepository();
        _uow = new FakeUnitOfWork();
        _cache = new FakeMemoryCache();

        _handler = new ConfirmStatementImportCommandHandler(
            _accountRepo,
            _importRepo,
            _txRepo,
            _catRepo,
            _ruleRepo,
            _uow,
            _cache
        );
    }

    [Fact]
    public async Task HandleAsync_ShouldAutoEnrichTransactionsWithMerchantAndCategories()
    {
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var previewToken = Guid.NewGuid();

        var account = new BankAccount { Id = accountId, UserId = userId, Name = "Cuenta Test" };
        _accountRepo.Accounts[accountId] = account;

        var catStreaming = new Category { Id = Guid.NewGuid(), Name = "Streaming Multimedia", NormalizedName = "STREAMING MULTIMEDIA" };
        var catSuscripciones = new Category { Id = Guid.NewGuid(), Name = "Suscripciones y Servicios", NormalizedName = "SUSCRIPCIONES Y SERVICIOS" };
        _catRepo.Categories[catStreaming.Id] = catStreaming;
        _catRepo.Categories[catSuscripciones.Id] = catSuscripciones;

        var rawTxList = new List<ParsedRawTransaction>
        {
            new(
                OperationDate: new DateOnly(2026, 9, 20),
                ValueDate: new DateOnly(2026, 9, 20),
                Description: "PAYPAL *NETFLIX 9283",
                Amount: -17.99m,
                Currency: "EUR",
                BalanceAfter: 1200.00m,
                BankCategory: null,
                BankSubcategory: null,
                BankMovementType: "Recibo",
                Observations: null,
                RawDataJson: "{}",
                DetectedCardLast4: null
            )
        };

        var parseResult = new StatementParseResult(
            BankType: BankType.Generic,
            Transactions: rawTxList,
            StartingBalance: 1217.99m,
            EndingBalance: 1200.00m,
            StartDate: new DateOnly(2026, 9, 20),
            EndDate: new DateOnly(2026, 9, 20),
            DetectedCards: new List<string>()
        );

        var cachedData = new CachedPreviewData(
            PreviewToken: previewToken,
            UserId: userId,
            BankAccountId: accountId,
            FileName: "test.csv",
            FileHashSha256: "test_hash",
            ParseResult: parseResult
        );

        _cache.Set(previewToken, cachedData);

        var cmd = new ConfirmStatementImportCommand(
            UserId: userId,
            BankAccountId: accountId,
            PreviewToken: previewToken
        );

        var result = await _handler.HandleAsync(cmd);

        Assert.True(result.Success);
        Assert.Single(_txRepo.Transactions);

        var tx = _txRepo.Transactions.Values.First();
        Assert.Equal("Netflix", tx.CleanMerchantName);
        Assert.Equal(ExternalPlatform.PayPal, tx.ExternalPlatform);
        Assert.Equal(CategorizationSource.SystemRule, tx.CategorizationSource);
        Assert.True(tx.IsRecurring);
        Assert.Equal(RecurrencePeriod.Monthly, tx.RecurrencePeriod);
        Assert.Equal(ExpenseFlexibility.Fixed, tx.ExpenseFlexibility);
    }
}

public class FakeMemoryCache : IMemoryCache
{
    public readonly Dictionary<object, object?> Store = new();

    public ICacheEntry CreateEntry(object key) => new FakeCacheEntry(key, this);

    public void Remove(object key) => Store.Remove(key);

    public bool TryGetValue(object key, out object? value) => Store.TryGetValue(key, out value);

    public void Dispose() { }
}

public class FakeCacheEntry : ICacheEntry
{
    private readonly FakeMemoryCache _cache;
    public object Key { get; }
    public object? Value { get; set; }
    public DateTimeOffset? AbsoluteExpiration { get; set; }
    public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }
    public TimeSpan? SlidingExpiration { get; set; }
    public IList<IChangeToken> ExpirationTokens => new List<IChangeToken>();
    public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks => new List<PostEvictionCallbackRegistration>();
    public CacheItemPriority Priority { get; set; }
    public long? Size { get; set; }

    public FakeCacheEntry(object key, FakeMemoryCache cache)
    {
        Key = key;
        _cache = cache;
    }

    public void Dispose()
    {
        _cache.Store[Key] = Value;
    }
}

public class FakeStatementImportRepository : IStatementImportRepository
{
    public readonly Dictionary<Guid, StatementImport> Imports = new();

    public Task<StatementImport?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        Imports.TryGetValue(id, out var imp);
        return Task.FromResult(imp);
    }

    public Task<List<StatementImport>> GetByAccountIdAsync(Guid accountId, CancellationToken ct = default)
    {
        return Task.FromResult(Imports.Values.Where(i => i.BankAccountId == accountId).ToList());
    }

    public Task<bool> ExistsHashForAccountAsync(Guid accountId, string fileHashSha256, CancellationToken ct = default)
    {
        var exists = Imports.Values.Any(i => i.BankAccountId == accountId && i.FileHashSha256 == fileHashSha256);
        return Task.FromResult(exists);
    }

    public Task AddAsync(StatementImport import, CancellationToken ct = default)
    {
        Imports[import.Id] = import;
        return Task.CompletedTask;
    }

    public void Remove(StatementImport import)
    {
        Imports.Remove(import.Id);
    }
}
