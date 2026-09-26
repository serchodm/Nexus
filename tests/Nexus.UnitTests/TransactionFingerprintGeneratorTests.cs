using Nexus.Domain.Services;
using Xunit;

namespace Nexus.UnitTests;

public class TransactionFingerprintGeneratorTests
{
    [Fact]
    public void Generate_DeterministicAndMatchesSameInputs()
    {
        var accountId = Guid.NewGuid();
        var date = new DateOnly(2026, 9, 15);
        var amount = -24.50m;
        var desc = "SUPERMERCADOS MERCADONA";
        var balance = 1500.25m;

        var fp1 = TransactionFingerprintGenerator.Generate(accountId, date, amount, desc, balance);
        var fp2 = TransactionFingerprintGenerator.Generate(accountId, date, amount, desc, balance);

        Assert.Equal(fp1, fp2);
        Assert.Equal(64, fp1.Length);
    }

    [Fact]
    public void Generate_NormalizesDescriptionWhitespace_YieldsSameFingerprint()
    {
        var accountId = Guid.NewGuid();
        var date = new DateOnly(2026, 9, 15);
        var amount = -24.50m;
        var desc1 = "SUPERMERCADOS   MERCADONA ";
        var desc2 = "SUPERMERCADOS MERCADONA";
        var balance = 1500.25m;

        var fp1 = TransactionFingerprintGenerator.Generate(accountId, date, amount, desc1, balance);
        var fp2 = TransactionFingerprintGenerator.Generate(accountId, date, amount, desc2, balance);

        Assert.Equal(fp1, fp2);
    }

    [Fact]
    public void Generate_DifferentAmountOrDate_YieldsDifferentFingerprints()
    {
        var accountId = Guid.NewGuid();
        var date1 = new DateOnly(2026, 9, 15);
        var date2 = new DateOnly(2026, 9, 16);
        var amount = -24.50m;
        var desc = "MERCADONA";
        var balance = 1500.25m;

        var fp1 = TransactionFingerprintGenerator.Generate(accountId, date1, amount, desc, balance);
        var fp2 = TransactionFingerprintGenerator.Generate(accountId, date2, amount, desc, balance);

        Assert.NotEqual(fp1, fp2);
    }
}
