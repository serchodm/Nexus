namespace Nexus.Domain.Services;

/// <summary>
/// Servicio de dominio para detección y correlación de traspasos internos entre cuentas propias.
/// </summary>
public static class InternalTransferMatcher
{
    /// <summary>
    /// Comprueba si dos movimientos bancarios de cuentas distintas corresponden a un traspaso propio.
    /// Uno debe ser un cargo (Amount &lt; 0) y el otro un abono (Amount &gt; 0) con importe idéntico y fechas cercanas.
    /// </summary>
    public static bool IsPotentialInternalTransfer(
        decimal amountA, 
        DateOnly dateA, 
        decimal amountB, 
        DateOnly dateB, 
        int maxDaysTolerance = 2)
    {
        // Deben tener signos opuestos (uno cargo, otro abono)
        if ((amountA < 0 && amountB <= 0) || (amountA > 0 && amountB >= 0))
        {
            return false;
        }

        // Importe exacto en valor absoluto
        if (Math.Abs(amountA) != Math.Abs(amountB))
        {
            return false;
        }

        // Ventana temporal de tolerancia (por ejemplo 2 días hábiles)
        var daysDifference = Math.Abs(dateA.DayNumber - dateB.DayNumber);
        return daysDifference <= maxDaysTolerance;
    }
}
