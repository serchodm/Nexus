using Nexus.Application.Commerce.Commands;
using Nexus.Domain.Entities;
using Nexus.Domain.Enums;
using Nexus.UnitTests.Fakes;
using Xunit;

namespace Nexus.UnitTests;

public class AcceptReconciliationCommandHandlerTests
{
    private readonly FakeTransactionReconciliationRepository _recRepo = new();
    private readonly FakeBankTransactionRepository _txRepo = new();
    private readonly FakeCategoryRepository _categoryRepo = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly AcceptReconciliationCommandHandler _handler;

    public AcceptReconciliationCommandHandlerTests()
    {
        _handler = new AcceptReconciliationCommandHandler(
            _recRepo,
            _txRepo,
            _categoryRepo,
            _unitOfWork
        );
    }

    [Fact]
    public async Task HandleAsync_ValidCandidate_CreatesSplitsWithZeroDiscrepancyAndSetsIsManual()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var defaultCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Compras",
            IsSystem = true
        };
        await _categoryRepo.AddAsync(defaultCategory);

        var tx = new BankTransaction
        {
            Id = Guid.NewGuid(),
            OperationDate = new DateOnly(2026, 9, 20),
            Amount = -45.49m,
            Description = "COMPRA AMAZON ES",
            CleanMerchantName = "Amazon",
            IsManual = false
        };
        _txRepo.Transactions[tx.Id] = tx;

        var order = new ExternalCommerceOrder
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Provider = CommerceProvider.Amazon,
            ExternalOrderId = "405-1234567-8901234",
            TotalAmount = 45.49m,
            Items = new List<ExternalCommerceOrderItem>
            {
                new() { Id = Guid.NewGuid(), Title = "Ratón ergonómico", TotalPrice = 29.99m },
                new() { Id = Guid.NewGuid(), Title = "Cable USB-C", TotalPrice = 15.50m }
            }
        };

        var rec = new TransactionReconciliation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            BankTransactionId = tx.Id,
            BankTransaction = tx,
            ExternalCommerceOrderId = order.Id,
            ExternalCommerceOrder = order,
            Status = ReconciliationStatus.Candidate,
            Confidence = MatchConfidence.High,
            MatchedOrderItemIds = order.Items.Select(i => i.Id).ToList()
        };
        await _recRepo.AddAsync(rec);

        // Act
        var result = await _handler.HandleAsync(new AcceptReconciliationCommand(userId, rec.Id));

        // Assert
        Assert.True(result.Success);
        Assert.Equal(2, result.CreatedSplitsCount);
        Assert.Equal(2, tx.SplitItems.Count);

        // Invariante RN-02: suma exacta 0.00 €
        var sumSplits = tx.SplitItems.Sum(s => s.Amount);
        Assert.Equal(Math.Abs(tx.Amount), sumSplits);

        // Invariante RN-01: IsManual = true para proteger contra sobreescritura
        Assert.True(tx.IsManual);
        Assert.True(tx.HasSplits);
        Assert.Equal("Amazon", tx.CleanMerchantName);
        Assert.Equal(ReconciliationStatus.Accepted, rec.Status);
    }

    [Fact]
    public async Task HandleAsync_ResidualShippingCost_AddsBalancingSplit()
    {
        // Arrange: Cargo bancario es 50.00€, pero los artículos suman 45.00€ (5.00€ de envío)
        var userId = Guid.NewGuid();
        var defaultCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Compras Generales",
            IsSystem = true
        };
        await _categoryRepo.AddAsync(defaultCategory);

        var tx = new BankTransaction
        {
            Id = Guid.NewGuid(),
            OperationDate = new DateOnly(2026, 9, 20),
            Amount = -50.00m,
            Description = "COMPRA AMAZON ES"
        };
        _txRepo.Transactions[tx.Id] = tx;

        var order = new ExternalCommerceOrder
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Provider = CommerceProvider.Amazon,
            ExternalOrderId = "405-SHIPPING",
            TotalAmount = 50.00m,
            Items = new List<ExternalCommerceOrderItem>
            {
                new() { Id = Guid.NewGuid(), Title = "Libro de Finanzas", TotalPrice = 45.00m }
            }
        };

        var rec = new TransactionReconciliation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            BankTransactionId = tx.Id,
            BankTransaction = tx,
            ExternalCommerceOrderId = order.Id,
            ExternalCommerceOrder = order,
            Status = ReconciliationStatus.Candidate,
            Confidence = MatchConfidence.High,
            MatchedOrderItemIds = order.Items.Select(i => i.Id).ToList()
        };
        await _recRepo.AddAsync(rec);

        // Act
        var result = await _handler.HandleAsync(new AcceptReconciliationCommand(userId, rec.Id));

        // Assert
        Assert.True(result.Success);
        Assert.Equal(2, tx.SplitItems.Count); // 1 artículo + 1 split de ajuste de envío
        Assert.Equal(50.00m, tx.SplitItems.Sum(s => s.Amount)); // 0.00 € discrepancia
        Assert.Contains(tx.SplitItems, s => s.Description!.Contains("Envío"));
    }
}
