using Nexus.Application.Transactions.Commands.SplitTransaction;
using Nexus.Application.Transactions.DTOs;
using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Nexus.Domain.Enums;
using Nexus.UnitTests.Fakes;
using Xunit;

namespace Nexus.UnitTests;

public class SplitTransactionCommandHandlerTests
{
    private readonly FakeBankTransactionRepository _txRepo;
    private readonly FakeBankAccountRepository _accountRepo;
    private readonly FakeCategoryRepository _categoryRepo;
    private readonly FakeUnitOfWork _uow;
    private readonly SplitTransactionCommandHandler _handler;

    public SplitTransactionCommandHandlerTests()
    {
        _txRepo = new FakeBankTransactionRepository();
        _accountRepo = new FakeBankAccountRepository();
        _categoryRepo = new FakeCategoryRepository();
        _uow = new FakeUnitOfWork();

        _handler = new SplitTransactionCommandHandler(
            _txRepo,
            _accountRepo,
            _categoryRepo,
            _uow
        );
    }

    [Fact]
    public async Task HandleAsync_WhenSumsDoNotMatch_ShouldThrowDomainValidationException()
    {
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var txId = Guid.NewGuid();
        var catId = Guid.NewGuid();

        var tx = new BankTransaction
        {
            Id = txId,
            BankAccountId = accountId,
            Amount = -100.00m,
            Description = "COMPRA MIXTA"
        };

        var account = new BankAccount { Id = accountId, UserId = userId };
        _accountRepo.Accounts[accountId] = account;
        _txRepo.Transactions[txId] = tx;

        var cmd = new SplitTransactionCommand(
            TransactionId: txId,
            UserId: userId,
            Items: new List<SplitItemInput>
            {
                new(catId, null, -60.00m, "Partida 1", ExpenseNature.Individual),
                new(catId, null, -30.00m, "Partida 2 (falta 10 €)", ExpenseNature.Shared)
            }
        );

        var ex = await Assert.ThrowsAsync<DomainValidationException>(() => _handler.HandleAsync(cmd));
        Assert.Contains("Invariante contable violada", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenValid_ShouldPersistSplitsAndMarkHasSplitsTrue()
    {
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var txId = Guid.NewGuid();
        var catId = Guid.NewGuid();

        var tx = new BankTransaction
        {
            Id = txId,
            BankAccountId = accountId,
            Amount = -100.00m,
            Description = "COMPRA MIXTA",
            SplitItems = new List<TransactionSplitItem>()
        };

        var account = new BankAccount { Id = accountId, UserId = userId };
        var category = new Category { Id = catId, Name = "Alimentación" };

        _accountRepo.Accounts[accountId] = account;
        _txRepo.Transactions[txId] = tx;
        _categoryRepo.Categories[catId] = category;

        var cmd = new SplitTransactionCommand(
            TransactionId: txId,
            UserId: userId,
            Items: new List<SplitItemInput>
            {
                new(catId, null, -70.00m, "Partida 1", ExpenseNature.Individual),
                new(catId, null, -30.00m, "Partida 2", ExpenseNature.Shared)
            }
        );

        var result = await _handler.HandleAsync(cmd);

        Assert.True(result.HasSplits);
        Assert.True(result.IsManual);
        Assert.Equal(2, result.SplitItems.Count);
        Assert.Equal(1, _uow.SaveChangesCalls);
    }
}
