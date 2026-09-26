using Nexus.Domain.Security;
using Xunit;

namespace Nexus.UnitTests;

public class TextSanitizerTests
{
    [Fact]
    public void Sanitize_ShouldCollapseMultipleWhitespace()
    {
        var input = "PAGO   EN   COMERCIO     MERCADONA ";
        var result = TextSanitizer.Sanitize(input);

        Assert.Equal("PAGO EN COMERCIO MERCADONA", result);
    }

    [Fact]
    public void Sanitize_ShouldMask16DigitPan()
    {
        var input = "PAGO TARJETA 4188123456786589 EN COMERCIO";
        var result = TextSanitizer.Sanitize(input);

        Assert.Equal("PAGO TARJETA **** **** **** 6589 EN COMERCIO", result);
    }

    [Fact]
    public void Sanitize_ShouldMask16DigitPan_WithHyphensOrSpaces()
    {
        var input = "4188-1234-5678-6589 PAGO TPV";
        var result = TextSanitizer.Sanitize(input);

        Assert.Equal("**** **** **** 6589 PAGO TPV", result);
    }

    [Fact]
    public void ExtractCardLast4_ShouldReturnCorrectDigits()
    {
        var text = "COMPRA TARJETA 4188123456786589 BARCELONA";
        var cards = TextSanitizer.ExtractCardLast4(text);

        Assert.Single(cards);
        Assert.Equal("6589", cards[0]);
    }

    [Fact]
    public void Sanitize_NullOrEmpty_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty, TextSanitizer.Sanitize(string.Empty));
        Assert.Equal(string.Empty, TextSanitizer.Sanitize(null));
    }
}
