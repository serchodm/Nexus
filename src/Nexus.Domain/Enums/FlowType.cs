namespace Nexus.Domain.Enums;

/// <summary>
/// Tipo de flujo financiero de una categoría o transacción.
/// </summary>
public enum FlowType
{
    Expense = 1,     // Gasto / Salida de fondos
    Income = 2,      // Ingreso / Entrada de fondos
    Transfer = 3     // Traspaso entre cuentas propias / Ajuste neutro
}
