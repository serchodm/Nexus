namespace Nexus.Domain.Enums;

/// <summary>
/// Estado del ciclo de vida contable de una partida compartida.
/// </summary>
public enum SharedExpenseStatus
{
    Pending = 1,   // Pendiente de liquidación; forma parte del balance deudor/acreedor
    Settled = 2    // Saldado/extinguido mediante liquidación Bizum, SEPA o compensación
}
