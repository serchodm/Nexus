namespace Nexus.Domain.Enums;

/// <summary>
/// Plataformas externas y pasarelas de pago online detectadas en transacciones bancarias.
/// </summary>
public enum ExternalPlatform
{
    None = 0,
    PayPal = 1,
    Amazon = 2,
    Apple = 3,
    GooglePay = 4,
    Klarna = 5,
    Bizum = 6
}
