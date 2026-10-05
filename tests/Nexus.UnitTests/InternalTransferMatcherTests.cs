using Nexus.Domain.Services;
using Xunit;

namespace Nexus.UnitTests;

public class InternalTransferMatcherTests
{
    [Fact]
    public void IsPotentialInternalTransfer_SameAmountOppositeSignsCloseDates_ReturnsTrue()
    {
        var amountA = -500.00m;
        var dateA = new DateOnly(2026, 10, 1);

        var amountB = 500.00m;
        var dateB = new DateOnly(2026, 10, 2);

        var isMatch = InternalTransferMatcher.IsPotentialInternalTransfer(amountA, dateA, amountB, dateB, maxDaysTolerance: 2);

        Assert.True(isMatch);
    }

    [Fact]
    public void IsPotentialInternalTransfer_SameSigns_ReturnsFalse()
    {
        var amountA = 500.00m;
        var dateA = new DateOnly(2026, 10, 1);

        var amountB = 500.00m;
        var dateB = new DateOnly(2026, 10, 2);

        var isMatch = InternalTransferMatcher.IsPotentialInternalTransfer(amountA, dateA, amountB, dateB);

        Assert.False(isMatch);
    }

    [Fact]
    public void IsPotentialInternalTransfer_DifferentAmounts_ReturnsFalse()
    {
        var amountA = -500.00m;
        var dateA = new DateOnly(2026, 10, 1);

        var amountB = 490.00m;
        var dateB = new DateOnly(2026, 10, 2);

        var isMatch = InternalTransferMatcher.IsPotentialInternalTransfer(amountA, dateA, amountB, dateB);

        Assert.False(isMatch);
    }

    [Fact]
    public void IsPotentialInternalTransfer_DatesTooFar_ReturnsFalse()
    {
        var amountA = -500.00m;
        var dateA = new DateOnly(2026, 10, 1);

        var amountB = 500.00m;
        var dateB = new DateOnly(2026, 10, 10);

        var isMatch = InternalTransferMatcher.IsPotentialInternalTransfer(amountA, dateA, amountB, dateB, maxDaysTolerance: 2);

        Assert.False(isMatch);
    }
}
