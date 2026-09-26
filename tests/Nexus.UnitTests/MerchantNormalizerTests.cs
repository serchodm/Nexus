using Nexus.Domain.Services;
using Xunit;

namespace Nexus.UnitTests;

public class MerchantNormalizerTests
{
    [Theory]
    [InlineData("TPV 49201 BCN MERCADONA S.A. 25/09", "Mercadona")]
    [InlineData("SUMUP *CAFETERIA PARIS", "Cafeteria Paris")]
    [InlineData("PAYPAL *NETFLIX 9238", "Netflix")]
    [InlineData("AMZN MKTP ES*H82Y192", "Amazon")]
    [InlineData("AMAZON.ES*H82Y192", "Amazon")]
    [InlineData("RECIBO DE IBERDROLA CLIENTES S.A.U.", "Iberdrola")]
    [InlineData("PAGO EN CABIFY MADRID ES 12/03", "Cabify")]
    [InlineData("COMPRA EN ZARA MADRID 28001", "Zara")]
    [InlineData("STRIPE *OPENAI 129381", "Openai")]
    [InlineData("REDYS TPV 1293 RESTAURANTE EL BODEGON", "Restaurante El Bodegon")]
    public void Normalize_ShouldExtractCleanMerchantName_FromRawBankNoise(string raw, string expected)
    {
        var result = MerchantNormalizer.Normalize(raw);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Normalize_NullOrEmpty_ShouldReturnNull()
    {
        Assert.Null(MerchantNormalizer.Normalize(null));
        Assert.Null(MerchantNormalizer.Normalize("   "));
    }
}
