using Nexus.Domain.Common;

namespace Nexus.Domain.Services;

/// <summary>
/// Validador de la invariante contable estricta de las transacciones desglosadas (RN-02).
/// La suma de todas las partidas desglosadas debe coincidir exactamente al céntimo con el importe de la transacción original.
/// </summary>
public static class TransactionSplitValidator
{
    public record SplitCandidate(
        decimal Amount,
        Guid CategoryId,
        Guid? SubcategoryId = null,
        string? Description = null
    );

    /// <summary>
    /// Valida que el conjunto de partidas cumpla la invariante de conservación contable.
    /// </summary>
    /// <param name="transactionAmount">Importe original de la transacción bancaria.</param>
    /// <param name="splits">Lista de partidas a validar.</param>
    /// <exception cref="DomainValidationException">Si la suma difiere o algún ítem es inválido.</exception>
    public static void Validate(decimal transactionAmount, IReadOnlyCollection<SplitCandidate>? splits)
    {
        if (splits == null || splits.Count == 0)
        {
            throw new DomainValidationException("Un desglose de transacción debe contener al menos una partida.");
        }

        if (splits.Count < 2)
        {
            throw new DomainValidationException("Un desglose de transacción debe contener al menos 2 partidas independientes.");
        }

        var totalSplits = 0m;
        var index = 1;

        foreach (var item in splits)
        {
            if (item.Amount == 0m)
            {
                throw new DomainValidationException($"La partida #{index} tiene un importe de 0,00 €. Todas las partidas deben tener un importe no nulo.");
            }

            if (item.CategoryId == Guid.Empty)
            {
                throw new DomainValidationException($"La partida #{index} no tiene una categoría válida asignada.");
            }

            totalSplits += item.Amount;
            index++;
        }

        // Validación estricta con precisión monetaria de 2 decimales (tolerancia 0.001)
        var difference = Math.Abs(transactionAmount - totalSplits);
        if (difference >= 0.005m)
        {
            throw new DomainValidationException(
                $"Invariante contable violada (RN-02): La suma de las partidas ({totalSplits:F2} €) no coincide con el total de la transacción ({transactionAmount:F2} €). Discrepancia: {(totalSplits - transactionAmount):F2} €.");
        }
    }
}
