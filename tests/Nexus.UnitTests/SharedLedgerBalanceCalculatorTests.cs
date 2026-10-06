using Nexus.Domain.Entities;
using Nexus.Domain.Enums;
using Nexus.Domain.Services;
using Xunit;

namespace Nexus.UnitTests;

public class SharedLedgerBalanceCalculatorTests
{
    [Fact]
    public void CalculateNetBalance_WithPendingMercadonaAndUtilities_ShouldEqualExact170_61()
    {
        // Arrange
        var sergioId = Guid.NewGuid();
        var elenaId = Guid.NewGuid();
        var spaceId = Guid.NewGuid();

        var pendingExpenses = new List<SharedExpenseEntry>
        {
            new()
            {
                SharedSpaceId = spaceId,
                PaidByMemberId = sergioId,
                Description = "Mercadona Gran Vía",
                TotalAmount = 120.00m,
                PaidByShare = 60.00m,
                OwedShare = 60.00m,
                Status = SharedExpenseStatus.Pending
            },
            new()
            {
                SharedSpaceId = spaceId,
                PaidByMemberId = sergioId,
                Description = "Agua + Luz + Gas Oct",
                TotalAmount = 245.80m,
                PaidByShare = 135.19m,
                OwedShare = 110.61m,
                Status = SharedExpenseStatus.Pending
            }
        };

        // Act - Verificación de corrección de bug FIN-03: Saldo exacto sin discrepancias
        var netBalance = SharedLedgerBalanceCalculator.CalculateNetBalance(pendingExpenses, sergioId);

        // Assert: 60.00 + 110.61 = 170.61 €
        Assert.Equal(170.61m, netBalance);
    }

    [Fact]
    public void CalculateNetBalance_BilateralExpenses_ShouldOffsetCorrectly()
    {
        // Arrange: Sergio paga 120 € (Elena debe 60 €), Elena paga 85 € (Sergio debe 42.50 €)
        var sergioId = Guid.NewGuid();
        var elenaId = Guid.NewGuid();
        var spaceId = Guid.NewGuid();

        var pendingExpenses = new List<SharedExpenseEntry>
        {
            new()
            {
                SharedSpaceId = spaceId,
                PaidByMemberId = sergioId,
                Description = "Supermercado",
                TotalAmount = 120.00m,
                PaidByShare = 60.00m,
                OwedShare = 60.00m,
                Status = SharedExpenseStatus.Pending
            },
            new()
            {
                SharedSpaceId = spaceId,
                PaidByMemberId = elenaId,
                Description = "Cena Restaurante",
                TotalAmount = 85.00m,
                PaidByShare = 42.50m,
                OwedShare = 42.50m,
                Status = SharedExpenseStatus.Pending
            }
        };

        // Act: Desde la perspectiva de Sergio
        var netBalanceSergio = SharedLedgerBalanceCalculator.CalculateNetBalance(pendingExpenses, sergioId);

        // Act: Desde la perspectiva de Elena
        var netBalanceElena = SharedLedgerBalanceCalculator.CalculateNetBalance(pendingExpenses, elenaId);

        // Assert: 60.00 - 42.50 = +17.50 € para Sergio, -17.50 € para Elena
        Assert.Equal(17.50m, netBalanceSergio);
        Assert.Equal(-17.50m, netBalanceElena);
        Assert.Equal(0.00m, netBalanceSergio + netBalanceElena);
    }

    [Fact]
    public void CalculateNetBalance_WhenAllSettled_ShouldBeZero()
    {
        // Arrange
        var sergioId = Guid.NewGuid();
        var spaceId = Guid.NewGuid();

        var settledExpenses = new List<SharedExpenseEntry>
        {
            new()
            {
                SharedSpaceId = spaceId,
                PaidByMemberId = sergioId,
                Description = "Gasto Pasado",
                TotalAmount = 100.00m,
                PaidByShare = 50.00m,
                OwedShare = 50.00m,
                Status = SharedExpenseStatus.Settled
            }
        };

        // Act
        var netBalance = SharedLedgerBalanceCalculator.CalculateNetBalance(settledExpenses, sergioId);

        // Assert
        Assert.Equal(0.00m, netBalance);
    }
}
