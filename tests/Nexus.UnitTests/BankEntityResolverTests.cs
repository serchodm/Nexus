using Nexus.Domain.Enums;
using Nexus.Domain.Services;
using Xunit;

namespace Nexus.UnitTests;

public class BankEntityResolverTests
{
    [Theory]
    [InlineData("ES9101821234567890123456", BankType.BBVA, "0182", "BBVA")]
    [InlineData("ES2114650100401234567890", BankType.ING, "1465", "ING Direct")]
    [InlineData("ES0000491234567890123456", BankType.Santander, "0049", "Banco Santander")]
    [InlineData("ES0021001234567890123456", BankType.CaixaBank, "2100", "CaixaBank")]
    [InlineData("ES0099991234567890123456", BankType.Generic, "9999", "Entidad 9999")]
    public void ResolveFromIban_ShouldIdentifyBanksCorrectly(string iban, BankType expectedBank, string expectedCode, string expectedName)
    {
        var (name, bankType, code) = BankEntityResolver.ResolveFromIban(iban);

        Assert.Equal(expectedBank, bankType);
        Assert.Equal(expectedCode, code);
        Assert.Equal(expectedName, name);
    }

    [Fact]
    public void ResolveFromIban_NullOrShort_ReturnsGeneric()
    {
        var (name, bankType, code) = BankEntityResolver.ResolveFromIban("ES12");
        Assert.Equal(BankType.Generic, bankType);
        Assert.Null(code);
    }
}
