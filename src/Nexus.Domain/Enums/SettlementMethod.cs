namespace Nexus.Domain.Enums;

/// <summary>
/// Método utilizado para saldar un balance pendiente en un espacio compartido.
/// </summary>
public enum SettlementMethod
{
    Bizum = 1,
    SepaTransfer = 2,
    Cash = 3,
    Offset = 4
}
