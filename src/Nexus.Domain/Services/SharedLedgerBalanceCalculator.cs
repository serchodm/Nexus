using Nexus.Domain.Entities;
using Nexus.Domain.Enums;

namespace Nexus.Domain.Services;

/// <summary>
/// Calculador del balance bilateral neto y resumen acumulado del libro compartido.
/// </summary>
public static class SharedLedgerBalanceCalculator
{
    /// <summary>
    /// Calcula el balance bilateral neto de un miembro respecto a su contraparte a partir de las partidas pendientes.
    /// </summary>
    /// <param name="pendingExpenses">Partidas en estado Pending</param>
    /// <param name="activeMemberId">Identificador del miembro activo que consulta</param>
    /// <returns>
    /// Importe decimal positivo si la contraparte le debe dinero (Receivable),
    /// negativo si el miembro activo debe dinero (Payable), o 0.00 € si están al día.
    /// </returns>
    public static decimal CalculateNetBalance(IEnumerable<SharedExpenseEntry> pendingExpenses, Guid activeMemberId)
    {
        decimal balance = 0m;

        foreach (var expense in pendingExpenses)
        {
            if (expense.Status != SharedExpenseStatus.Pending)
            {
                continue;
            }

            if (expense.PaidByMemberId == activeMemberId)
            {
                // El usuario pagó: la contraparte le debe la cuota adeudada (OwedShare)
                balance += expense.OwedShare;
            }
            else
            {
                // La contraparte pagó: el usuario le debe la cuota adeudada (OwedShare)
                balance -= expense.OwedShare;
            }
        }

        return Math.Round(balance, 2, MidpointRounding.ToEven);
    }

    /// <summary>
    /// Desglosa el volumen total compartido en el periodo y la aportación neta de cada miembro.
    /// </summary>
    public static (decimal TotalShared, decimal UserPaidTotal, decimal PartnerPaidTotal) CalculateTotals(
        IEnumerable<SharedExpenseEntry> expenses,
        Guid activeMemberId)
    {
        decimal total = 0m;
        decimal userPaid = 0m;
        decimal partnerPaid = 0m;

        foreach (var expense in expenses)
        {
            total += expense.TotalAmount;
            if (expense.PaidByMemberId == activeMemberId)
            {
                userPaid += expense.TotalAmount;
            }
            else
            {
                partnerPaid += expense.TotalAmount;
            }
        }

        return (
            Math.Round(total, 2, MidpointRounding.ToEven),
            Math.Round(userPaid, 2, MidpointRounding.ToEven),
            Math.Round(partnerPaid, 2, MidpointRounding.ToEven)
        );
    }
}
