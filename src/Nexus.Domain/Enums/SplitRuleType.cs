namespace Nexus.Domain.Enums;

/// <summary>
/// Tipo de regla de reparto para gastos comunes en un espacio compartido.
/// </summary>
public enum SplitRuleType
{
    Equal5050 = 1,             // 50% / 50% simétrico
    FixedRatio = 2,            // Ratio fijo (ej. 55% / 45%)
    ProportionalToIncome = 3   // Ponderado según los ingresos netos declarados (Fair-Share)
}
