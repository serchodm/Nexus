using System.Text.RegularExpressions;

namespace Nexus.Domain.Security;

public static class TextSanitizer
{
    private static readonly Regex PanRegex = new(
        @"\b(?:\d{4}[ -]?){3}\d{4}\b",
        RegexOptions.Compiled);

    private static readonly Regex MultipleWhitespaceRegex = new(
        @"\s+",
        RegexOptions.Compiled);

    /// <summary>
    /// Colapsa espacios consecutivos y enmascara números de tarjeta (PAN de 16 dígitos) a **** **** **** 1234.
    /// </summary>
    public static string Sanitize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        // 1. Colapsar espacios múltiples a un solo espacio
        var collapsed = MultipleWhitespaceRegex.Replace(text.Trim(), " ");

        // 2. Enmascarar números de tarjeta de 16 dígitos
        return PanRegex.Replace(collapsed, match =>
        {
            var digitsOnly = Regex.Replace(match.Value, @"\D", "");
            if (digitsOnly.Length != 16) return match.Value;
            var last4 = digitsOnly[^4..];
            return $"**** **** **** {last4}";
        });
    }

    /// <summary>
    /// Extrae los últimos 4 dígitos de cualquier tarjeta de 16 dígitos detectada en el texto.
    /// </summary>
    public static List<string> ExtractCardLast4(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return new List<string>();

        var cards = new List<string>();
        var matches = PanRegex.Matches(text);
        foreach (Match match in matches)
        {
            var digitsOnly = Regex.Replace(match.Value, @"\D", "");
            if (digitsOnly.Length == 16)
            {
                var last4 = digitsOnly[^4..];
                if (!cards.Contains(last4))
                {
                    cards.Add(last4);
                }
            }
        }
        return cards;
    }

    /// <summary>
    /// Normaliza descripción para cálculo determinista de hash fingerprint.
    /// </summary>
    public static string NormalizeForFingerprint(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        var clean = MultipleWhitespaceRegex.Replace(text.Trim(), " ").ToUpperInvariant();
        return PanRegex.Replace(clean, match =>
        {
            var digitsOnly = Regex.Replace(match.Value, @"\D", "");
            if (digitsOnly.Length != 16) return match.Value;
            return $"CARD_{digitsOnly[^4..]}";
        });
    }
}
