using Nexus.Domain.Common;
using Nexus.Domain.Services;
using Xunit;

namespace Nexus.UnitTests;

public class TransactionSplitValidatorTests
{
    [Fact]
    public void Validate_WhenSumsMatchExactly_ShouldSucceed()
    {
        var splits = new List<TransactionSplitValidator.SplitCandidate>
        {
            new(-80.00m, Guid.NewGuid(), null, "Comida"),
            new(-40.00m, Guid.NewGuid(), null, "Menaje")
        };

        var exception = Record.Exception(() => TransactionSplitValidator.Validate(-120.00m, splits));

        Assert.Null(exception);
    }

    [Fact]
    public void Validate_WhenSumDiffersByOneCent_ShouldThrowDomainValidationException()
    {
        var splits = new List<TransactionSplitValidator.SplitCandidate>
        {
            new(-80.00m, Guid.NewGuid(), null, "Comida"),
            new(-39.99m, Guid.NewGuid(), null, "Menaje")
        };

        var ex = Assert.Throws<DomainValidationException>(() => TransactionSplitValidator.Validate(-120.00m, splits));
        Assert.Contains("Invariante contable violada", ex.Message);
    }

    [Fact]
    public void Validate_WhenSingleItemProvided_ShouldThrowDomainValidationException()
    {
        var splits = new List<TransactionSplitValidator.SplitCandidate>
        {
            new(-120.00m, Guid.NewGuid(), null, "Todo")
        };

        var ex = Assert.Throws<DomainValidationException>(() => TransactionSplitValidator.Validate(-120.00m, splits));
        Assert.Contains("al menos 2 partidas independientes", ex.Message);
    }

    [Fact]
    public void Validate_WhenZeroAmountItemProvided_ShouldThrowDomainValidationException()
    {
        var splits = new List<TransactionSplitValidator.SplitCandidate>
        {
            new(-120.00m, Guid.NewGuid(), null, "Comida"),
            new(0.00m, Guid.NewGuid(), null, "Vacio")
        };

        var ex = Assert.Throws<DomainValidationException>(() => TransactionSplitValidator.Validate(-120.00m, splits));
        Assert.Contains("importe de 0,00 €", ex.Message);
    }
}
