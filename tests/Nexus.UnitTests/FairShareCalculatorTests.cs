using Nexus.Domain.Services;
using Xunit;

namespace Nexus.UnitTests;

public class FairShareCalculatorTests
{
    [Theory]
    [InlineData(245.80, 55.0, 135.19, 110.61)]
    [InlineData(120.00, 50.0, 60.00, 60.00)]
    [InlineData(80.00, 50.0, 40.00, 40.00)]
    [InlineData(100.01, 55.0, 55.01, 45.00)]
    [InlineData(99.99, 50.0, 50.00, 49.99)]
    [InlineData(0.01, 50.0, 0.00, 0.01)]
    [InlineData(1000.00, 60.0, 600.00, 400.00)]
    public void CalculateShares_ShouldSatisfyZeroCentDiscrepancyInvariant(
        decimal totalAmount,
        decimal payerPercentage,
        decimal expectedPayer,
        decimal expectedDebtor)
    {
        // Act
        var (payerShare, debtorShare) = FairShareCalculator.CalculateShares(totalAmount, payerPercentage);

        // Assert - Verificación de Invariante Contable RN-01: Suma exacta sin céntimos huérfanos
        Assert.Equal(totalAmount, payerShare + debtorShare);
        Assert.Equal(expectedPayer, payerShare);
        Assert.Equal(expectedDebtor, debtorShare);
    }

    [Fact]
    public void CalculateIncomeProportions_WithSergioAndElenaIncomes_ShouldReturn55And45()
    {
        // Arrange: Sergio 2.750 €, Elena 2.250 €
        decimal sergioIncome = 2750.00m;
        decimal elenaIncome = 2250.00m;

        // Act
        var (ownerPct, memberPct) = FairShareCalculator.CalculateIncomeProportions(sergioIncome, elenaIncome);

        // Assert
        Assert.Equal(55.00m, ownerPct);
        Assert.Equal(45.00m, memberPct);
        Assert.Equal(100.00m, ownerPct + memberPct);
    }

    [Fact]
    public void CalculateIncomeProportions_ZeroIncomes_ShouldFallbackTo5050()
    {
        // Act
        var (ownerPct, memberPct) = FairShareCalculator.CalculateIncomeProportions(0m, 0m);

        // Assert
        Assert.Equal(50.00m, ownerPct);
        Assert.Equal(50.00m, memberPct);
    }
}
