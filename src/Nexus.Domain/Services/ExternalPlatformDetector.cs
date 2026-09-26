using System.Text.RegularExpressions;
using Nexus.Domain.Enums;

namespace Nexus.Domain.Services;

/// <summary>
/// Detector de pasarelas de pago y plataformas externas en transacciones bancarias.
/// </summary>
public static class ExternalPlatformDetector
{
    private static readonly Regex PayPalOpaqueRegex = new(
        @"PAYPAL\s*\*([A-Z0-9]{8,})",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex AmazonOpaqueRegex = new(
        @"(AMZN\s*MKTP|AMAZON)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>
    /// Detecta la plataforma externa y evalúa si requiere reconciliación posterior con conectores.
    /// </summary>
    public static (ExternalPlatform Platform, bool PendingEnrichment) Detect(string? description, string? movementType = null)
    {
        if (string.IsNullOrWhiteSpace(description))
            return (ExternalPlatform.None, false);

        var upper = description.ToUpperInvariant();
        var typeUpper = movementType?.ToUpperInvariant() ?? string.Empty;

        // 1. Bizum
        if (upper.Contains("BIZUM") || typeUpper.Contains("BIZUM"))
        {
            return (ExternalPlatform.Bizum, false);
        }

        // 2. PayPal
        if (upper.Contains("PAYPAL"))
        {
            // Si es un código alfanumérico opaco de PayPal (ej. PAYPAL *35314369001 o PAYPAL *7H921), requiere reconciliación
            var isOpaque = PayPalOpaqueRegex.IsMatch(upper);
            return (ExternalPlatform.PayPal, isOpaque);
        }

        // 3. Amazon
        if (upper.Contains("AMAZON") || upper.Contains("AMZN"))
        {
            // Los cargos de Amazon en extracto frecuentemente no desglosan los productos comprados
            return (ExternalPlatform.Amazon, true);
        }

        // 4. Apple
        if (upper.Contains("APPLE.COM") || upper.Contains("APPLE PAY") || upper.Contains("ITUNES"))
        {
            return (ExternalPlatform.Apple, false);
        }

        // 5. Google
        if (upper.Contains("GOOGLE") || upper.Contains("GPAY"))
        {
            return (ExternalPlatform.GooglePay, false);
        }

        // 6. Klarna
        if (upper.Contains("KLARNA"))
        {
            return (ExternalPlatform.Klarna, true);
        }

        return (ExternalPlatform.None, false);
    }
}
