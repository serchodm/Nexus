namespace Nexus.Domain.Enums;

/// <summary>
/// Naturaleza contable y semántica del movimiento financiero.
/// </summary>
public enum TransactionNature
{
    /// <summary>Gasto ordinario que computa en el presupuesto de salidas.</summary>
    StandardExpense = 0,

    /// <summary>Abono o contra-partida que minora el gasto de una categoría (devoluciones, Bizums de gastos compartidos).</summary>
    ExpenseReimbursement = 1,

    /// <summary>Ingreso real genuino que incrementa el patrimonio neto (nómina, rentas, rendimientos, ventas).</summary>
    TrueIncome = 2,

    /// <summary>Movimiento neutro entre cuentas propias del usuario (cero impacto en P&L).</summary>
    InternalTransfer = 3
}
