namespace Nexus.Domain.Services;

/// <summary>
/// Calculador puro de cuotas de reparto progresivo y Fair-Share con redondeo bancario estricto.
/// Invariante Contable: PayerShare + DebtorShare == TotalAmount (Cero Tolerancia a céntimos huérfanos).
/// </summary>
public static class FairShareCalculator
{
    /// <summary>
    /// Calcula las cuotas exactas para un gasto compartido dado un porcentaje para el pagador.
    /// </summary>
    /// <param name="totalAmount">Importe total del gasto</param>
    /// <param name="payerPercentage">Porcentaje que asume el pagador (ej. 55.00m)</param>
    /// <returns>Tupla con la cuota del pagador y la cuota adeudada por la contraparte</returns>
    public static (decimal PayerShare, decimal DebtorShare) CalculateShares(decimal totalAmount, decimal payerPercentage)
    {
        if (totalAmount < 0)
        {
            totalAmount = Math.Abs(totalAmount);
        }

        if (payerPercentage < 0 || payerPercentage > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(payerPercentage), "El porcentaje debe estar comprendido entre 0 y 100.");
        }

        // Banker's Rounding (Round-to-even) para precisión contable institucional
        decimal payerShare = Math.Round(totalAmount * (payerPercentage / 100m), 2, MidpointRounding.ToEven);
        
        // La cuota del deudor se obtiene por sustracción estricta para garantizar 0.00 € de discrepancia
        decimal debtorShare = totalAmount - payerShare;

        return (payerShare, debtorShare);
    }

    /// <summary>
    /// Calcula los porcentajes ponderados proporcionales a los ingresos netos mensuales (Fair-Share).
    /// </summary>
    public static (decimal OwnerPercentage, decimal MemberPercentage) CalculateIncomeProportions(decimal ownerIncome, decimal memberIncome)
    {
        decimal totalIncome = ownerIncome + memberIncome;

        if (totalIncome <= 0)
        {
            return (50.00m, 50.00m);
        }

        decimal ownerPct = Math.Round((ownerIncome / totalIncome) * 100m, 2, MidpointRounding.ToEven);
        decimal memberPct = 100.00m - ownerPct;

        return (ownerPct, memberPct);
    }
}
