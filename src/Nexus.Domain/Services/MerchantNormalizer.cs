using System.Globalization;
using System.Text.RegularExpressions;

namespace Nexus.Domain.Services;

/// <summary>
/// Normalizador de comercios que purga el ruido de terminales TPV, pasarelas de pago,
/// sufijos legales y datos de tarjeta para obtener un nombre legible y estandarizado.
/// </summary>
public static partial class MerchantNormalizer
{
    private static readonly Regex PrefixNoiseRegex = new(
        @"^(TPV\s*\d*|SUMUP\s*\*?|PAYPAL\s*\*?|STRIPE\s*\*?|REDYS\s*\*?|RSD\s*\*?|COMPRA\s+EN\s+|PAGO\s+EN\s+|RECIBO\s+DE\s+|ADEUDO\s+DE\s+|TRANSFERENCIA\s+(DE|A)?\s*|BIZUM\s+(DE|A)?\s*)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex AmazonPrefixRegex = new(
        @"^(AMZN\s*MKTP\s*(ES)?\*?|AMAZON\.ES\*?|AMZN\s*)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex LegalSuffixRegex = new(
        @"\b(S\.?A\.?U\.?|S\.?L\.?U\.?|S\.?A\.?|S\.?L\.?|S\.?C\.?P\.?|SOCIEDAD\s+ANONIMA|SOCIEDAD\s+LIMITADA)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex DateNoiseRegex = new(
        @"\b\d{2}[\/\.-]\d{2}([\/\.-]\d{2,4})?\b",
        RegexOptions.Compiled);

    private static readonly Regex PostalCodeRegex = new(
        @"\b\d{5}\b",
        RegexOptions.Compiled);

    private static readonly Regex LongIdentifierRegex = new(
        @"\b\d{6,}\b",
        RegexOptions.Compiled);

    private static readonly Regex CountryOrCitySuffixRegex = new(
        @"\b(ES|ESP|SPAIN)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex MultipleSpacesRegex = new(
        @"\s+",
        RegexOptions.Compiled);

    /// <summary>
    /// Limpia y normaliza la descripción cruda de una transacción bancaria para extraer el nombre del comercio.
    /// </summary>
    /// <param name="rawDescription">Descripción bancaria de origen.</param>
    /// <returns>Nombre limpio del comercio en formato Title Case o null si la entrada es vacía.</returns>
    public static string? Normalize(string? rawDescription)
    {
        if (string.IsNullOrWhiteSpace(rawDescription))
            return null;

        var text = rawDescription.Trim();

        // 1. Detectar prefijo específico de Amazon
        if (AmazonPrefixRegex.IsMatch(text))
        {
            var withoutAmazonPrefix = AmazonPrefixRegex.Replace(text, string.Empty).Trim();
            // Si tras quitar el prefijo de Amazon queda un código de pedido opaco o nada, normalizar directamente a Amazon
            if (string.IsNullOrWhiteSpace(withoutAmazonPrefix) || Regex.IsMatch(withoutAmazonPrefix, @"^[A-Z0-9\*\-]+$"))
            {
                return "Amazon";
            }
            text = withoutAmazonPrefix;
        }

        // 2. Eliminar prefijos de pasarelas de pago y terminales TPV (pueden venir encadenados como "REDYS TPV 1293")
        while (PrefixNoiseRegex.IsMatch(text))
        {
            var replaced = PrefixNoiseRegex.Replace(text, string.Empty).Trim();
            if (replaced == text) break;
            text = replaced;
        }

        // 3. Eliminar fechas intercaladas (ej. 25/09 o 12/03/24)
        text = DateNoiseRegex.Replace(text, string.Empty);

        // 4. Eliminar sufijos de forma jurídica societaria (S.A., S.L., etc.)
        text = LegalSuffixRegex.Replace(text, string.Empty);

        // 5. Eliminar códigos postales
        text = PostalCodeRegex.Replace(text, string.Empty);

        // 6. Eliminar identificadores numéricos largos de terminal
        text = LongIdentifierRegex.Replace(text, string.Empty);

        // 7. Eliminar código de país final (ES, ESP)
        text = CountryOrCitySuffixRegex.Replace(text, string.Empty);

        // 8. Limpiar caracteres de puntuación residuales como asteriscos, barras o guiones sobrantes
        text = Regex.Replace(text, @"[*#_|~]+", " ");
        text = MultipleSpacesRegex.Replace(text, " ").Trim(' ', '-', ',', '.');

        if (string.IsNullOrWhiteSpace(text))
        {
            return "Comercio Genérico";
        }

        // 9. Capitalizar a Title Case limpio respetando excepciones conocidas
        return FormatToTitleCase(text);
    }

    private static string FormatToTitleCase(string input)
    {
        var upper = input.ToUpperInvariant();

        // Marcas conocidas con mayúsculas/minúsculas canónicas
        if (upper.Contains("MERCADONA")) return "Mercadona";
        if (upper.Contains("CARREFOUR")) return "Carrefour";
        if (upper.Contains("LIDL")) return "Lidl";
        if (upper.Contains("ALDI")) return "Aldi";
        if (upper.Contains("NETFLIX")) return "Netflix";
        if (upper.Contains("SPOTIFY")) return "Spotify";
        if (upper.Contains("CABIFY")) return "Cabify";
        if (upper.Contains("UBER")) return upper.Contains("EATS") ? "Uber Eats" : "Uber";
        if (upper.Contains("GLOVO")) return "Glovo";
        if (upper.Contains("JUST EAT")) return "Just Eat";
        if (upper.Contains("AMAZON")) return "Amazon";
        if (upper.Contains("APPLE")) return "Apple";
        if (upper.Contains("IBERDROLA")) return "Iberdrola";
        if (upper.Contains("ENDESA")) return "Endesa";
        if (upper.Contains("NATURGY")) return "Naturgy";
        if (upper.Contains("VODAFONE")) return "Vodafone";
        if (upper.Contains("MOVISTAR")) return "Movistar";
        if (upper.Contains("ORANGE")) return "Orange";
        if (upper.Contains("REPSOL")) return "Repsol";
        if (upper.Contains("CEPSA")) return "Cepsa";
        if (upper.Contains("BP")) return "BP";
        if (upper.Contains("ZARA")) return "Zara";
        if (upper.Contains("DECATHLON")) return "Decathlon";
        if (upper.Contains("LEROY MERLIN")) return "Leroy Merlin";
        if (upper.Contains("IKEA")) return "IKEA";
        if (upper.Contains("EL CORTE INGLES")) return "El Corte Inglés";

        var textInfo = CultureInfo.CurrentCulture.TextInfo;
        return textInfo.ToTitleCase(input.ToLowerInvariant());
    }
}
