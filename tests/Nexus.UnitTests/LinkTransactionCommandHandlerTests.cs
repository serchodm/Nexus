using Nexus.Application.Transactions.Commands.LinkTransaction;
using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Nexus.Domain.Enums;
using Nexus.UnitTests.Fakes;
using Xunit;

namespace Nexus.UnitTests;

public class LinkTransactionCommandHandlerTests
{
    private readonly FakeBankTransactionRepository _txRepo;
    private readonly FakeBankAccountRepository _accountRepo;
    private readonly FakeUnitOfWork _uow;
    private readonly LinkTransactionCommandHandler _handler;

    public LinkTransactionCommandHandlerTests()
    {
        _txRepo = new FakeBankTransactionRepository();
        _accountRepo = new FakeBankAccountRepository();
        _uow = new FakeUnitOfWork();

        _handler = new LinkTransactionCommandHandler(
            _txRepo,
            _accountRepo,
            _uow
        );
    }

    [Fact]
    public async Task HandleAsync_ValidRefund_LinksAndSetsExpenseReimbursement()
    {
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var originalTxId = Guid.NewGuid();
        var refundTxId = Guid.NewGuid();
        var catId = Guid.NewGuid();

        var account = new BankAccount { Id = accountId, UserId = userId };
        _accountRepo.Accounts[accountId] = account;

        var originalTx = new BankTransaction
        {
            Id = originalTxId,
            BankAccountId = accountId,
            Amount = -120.00m,
            Description = "COMPRA ZARA",
            CleanMerchantName = "Zara",
            CategoryId = catId
        };

        var refundTx = new BankTransaction
        {
            Id = refundTxId,
            BankAccountId = accountId,
            Amount = 40.00m,
            Description = "ABONO ZARA"
        };

        _txRepo.Transactions[originalTxId] = originalTx;
        _txRepo.Transactions[refundTxId] = refundTx;

        var cmd = new LinkTransactionCommand(refundTxId, originalTxId, userId);
        var result = await _handler.HandleAsync(cmd);

        Assert.Equal(TransactionNature.ExpenseReimbursement, result.TransactionNature);
        Assert.Equal(originalTxId, result.LinkedTransactionId);
        Assert.Equal("Zara", result.CleanMerchantName);
        Assert.Equal(catId, result.CategoryId);
    }

    [Fact]
    public async Task HandleAsync_RefundAmountExceedsOriginalCharge_ThrowsDomainValidationException()
    {
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var originalTxId = Guid.NewGuid();
        var refundTxId = Guid.NewGuid();

        var account = new BankAccount { Id = accountId, UserId = userId };
        _accountRepo.Accounts[accountId] = account;

        var originalTx = new BankTransaction
        {
            Id = originalTxId,
            BankAccountId = accountId,
            Amount = -50.00m,
            Description = "COMPRA CENA"
        };

        var refundTx = new BankTransaction
        {
            Id = refundTxId,
            BankAccountId = accountId,
            Amount = 60.00m, // Supera el gasto original
            Description = "BIZUM"
        };

        _txRepo.Transactions[originalTxId] = originalTx;
        _txRepo.Transactions[refundTxId] = refundTx;

        var cmd = new LinkTransactionCommand(refundTxId, originalTxId, userId);

        await Assert.ThrowsAsync<DomainValidationException>(() => _handler.HandleAsync(cmd));
    }

    [Fact]
    public async Task HandleAsync_ValidInternalTransfer_MarksBothAsInternalTransfer()
    {
        var userId = Guid.NewGuid();
        var accountId1 = Guid.NewGuid();
        var accountId2 = Guid.NewGuid();
        var outgoingTxId = Guid.NewGuid();
        var incomingTxId = Guid.NewGuid();

        _accountRepo.Accounts[accountId1] = new BankAccount { Id = accountId1, UserId = userId };
        _accountRepo.Accounts[accountId2] = new BankAccount { Id = accountId2, UserId = userId };

        var outgoingTx = new BankTransaction
        {
            Id = outgoingTxId,
            BankAccountId = accountId1,
            Amount = -500.00m,
            Description = "TRASPASO A CUENTA ING"
        };

        var incomingTx = new BankTransaction
        {
            Id = incomingTxId,
            BankAccountId = accountId2,
            Amount = 500.00m,
            Description = "TRASPASO DESDE BBVA"
        };

        _txRepo.Transactions[outgoingTxId] = outgoingTx;
        _txRepo.Transactions[incomingTxId] = incomingTx;

        var cmd = new LinkTransactionCommand(outgoingTxId, incomingTxId, userId);
        var result = await _handler.HandleAsync(cmd);

        Assert.Equal(TransactionNature.InternalTransfer, result.TransactionNature);
        Assert.Equal(incomingTxId, result.LinkedTransactionId);
        Assert.Equal(TransactionNature.InternalTransfer, incomingTx.TransactionNature);
        Assert.Equal(outgoingTxId, incomingTx.LinkedTransactionId);
    }
}
