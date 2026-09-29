using Nexus.Domain.Entities;
using Nexus.Domain.Enums;
using Nexus.Domain.Services;
using Xunit;

namespace Nexus.UnitTests;

public class CommerceReconciliationEngineTests
{
    private readonly CommerceReconciliationEngine _engine = new();

    [Fact]
    public void FindBestMatch_ExactAmazonOrder_ReturnsHighConfidenceMatch()
    {
        // Arrange
        var tx = new BankTransaction
        {
            Id = Guid.NewGuid(),
            OperationDate = new DateOnly(2026, 9, 16),
            Amount = -45.49m,
            Description = "COMPRA AMAZON ES MADRID"
        };

        var order = new ExternalCommerceOrder
        {
            Id = Guid.NewGuid(),
            Provider = CommerceProvider.Amazon,
            ExternalOrderId = "405-1234567-8901234",
            OrderDate = new DateTime(2026, 9, 15, 10, 0, 0, DateTimeKind.Utc),
            TotalAmount = 45.49m,
            Items = new List<ExternalCommerceOrderItem>
            {
                new() { Id = Guid.NewGuid(), Title = "Ratón", TotalPrice = 29.99m },
                new() { Id = Guid.NewGuid(), Title = "Cable USB", TotalPrice = 15.50m }
            }
        };

        // Act
        var candidate = _engine.FindBestMatch(tx, new[] { order });

        // Assert
        Assert.NotNull(candidate);
        Assert.Equal(MatchConfidence.Exact100, candidate.Confidence);
        Assert.Equal(order.Id, candidate.ExternalCommerceOrderId);
        Assert.Equal(2, candidate.MatchedOrderItemIds.Count);
    }

    [Fact]
    public void FindBestMatch_OutsideTemporalWindow_ReturnsNull()
    {
        // Arrange (15 days difference > 7 days max allowed)
        var tx = new BankTransaction
        {
            Id = Guid.NewGuid(),
            OperationDate = new DateOnly(2026, 10, 5),
            Amount = -45.49m,
            Description = "COMPRA AMAZON ES MADRID"
        };

        var order = new ExternalCommerceOrder
        {
            Id = Guid.NewGuid(),
            Provider = CommerceProvider.Amazon,
            ExternalOrderId = "405-1234567-8901234",
            OrderDate = new DateTime(2026, 9, 15, 10, 0, 0, DateTimeKind.Utc),
            TotalAmount = 45.49m
        };

        // Act
        var candidate = _engine.FindBestMatch(tx, new[] { order });

        // Assert
        Assert.Null(candidate);
    }

    [Fact]
    public void FindBestMatch_PayPalCounterparty_MatchesWithHighConfidence()
    {
        // Arrange
        var tx = new BankTransaction
        {
            Id = Guid.NewGuid(),
            OperationDate = new DateOnly(2026, 9, 15),
            Amount = -12.99m,
            Description = "RECIBO PAYPAL *SPOTIFY MADRID"
        };

        var order = new ExternalCommerceOrder
        {
            Id = Guid.NewGuid(),
            Provider = CommerceProvider.PayPal,
            ExternalOrderId = "PP-998877",
            OrderDate = new DateTime(2026, 9, 14, 20, 0, 0, DateTimeKind.Utc),
            TotalAmount = 12.99m,
            CounterpartyName = "Spotify AB",
            Items = new List<ExternalCommerceOrderItem>
            {
                new() { Id = Guid.NewGuid(), Title = "Spotify Premium", TotalPrice = 12.99m }
            }
        };

        // Act
        var candidate = _engine.FindBestMatch(tx, new[] { order });

        // Assert
        Assert.NotNull(candidate);
        Assert.Equal(MatchConfidence.Exact100, candidate.Confidence);
        Assert.Contains("Spotify AB", candidate.MatchReason);
    }

    [Fact]
    public void FindBestMatch_SubsetSum_AmazonPartialShipment_MatchesCorrectItems()
    {
        // Arrange: Pedido de 70€ con dos artículos (20€ y 50€). El banco tiene un cargo de 50€.
        var item1 = new ExternalCommerceOrderItem { Id = Guid.NewGuid(), Title = "Funda Móvil", TotalPrice = 20.00m };
        var item2 = new ExternalCommerceOrderItem { Id = Guid.NewGuid(), Title = "Auriculares", TotalPrice = 50.00m };

        var order = new ExternalCommerceOrder
        {
            Id = Guid.NewGuid(),
            Provider = CommerceProvider.Amazon,
            ExternalOrderId = "405-PARTIAL-SPLIT",
            OrderDate = new DateTime(2026, 9, 20, 12, 0, 0, DateTimeKind.Utc),
            TotalAmount = 70.00m,
            Items = new List<ExternalCommerceOrderItem> { item1, item2 }
        };

        var tx = new BankTransaction
        {
            Id = Guid.NewGuid(),
            OperationDate = new DateOnly(2026, 9, 21),
            Amount = -50.00m,
            Description = "AMAZON PAYMENTS LUXEMBOURG"
        };

        // Act
        var candidate = _engine.FindBestMatch(tx, new[] { order });

        // Assert
        Assert.NotNull(candidate);
        Assert.Single(candidate.MatchedOrderItemIds);
        Assert.Equal(item2.Id, candidate.MatchedOrderItemIds[0]);
    }
}
