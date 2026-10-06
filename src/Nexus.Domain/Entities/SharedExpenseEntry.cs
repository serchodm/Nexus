using Nexus.Domain.Common;
using Nexus.Domain.Enums;

namespace Nexus.Domain.Entities;

/// <summary>
/// Partida de gasto imputada al libro compartido de un hogar o pareja.
/// </summary>
public class SharedExpenseEntry : BaseEntity
{
    public Guid SharedSpaceId { get; set; }
    public SharedSpace SharedSpace { get; set; } = null!;

    // Enlace opcional a transacción bancaria directa o a desglose de ticket (RN-02)
    public Guid? BankTransactionId { get; set; }
    public BankTransaction? BankTransaction { get; set; }

    public Guid? TransactionSplitItemId { get; set; }
    public TransactionSplitItem? TransactionSplitItem { get; set; }

    public Guid PaidByMemberId { get; set; }
    public SharedSpaceMember PaidByMember { get; set; } = null!;

    public string Description { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    
    public decimal TotalAmount { get; set; }
    public decimal PaidByShare { get; set; } // Cuota correspondiente al pagador (consumo propio)
    public decimal OwedShare { get; set; }   // Cuota adeudada por la contraparte (cuentas por cobrar)

    public string CategoryName { get; set; } = "General";
    public string CategoryIcon { get; set; } = "tag";
    
    public SharedExpenseStatus Status { get; set; } = SharedExpenseStatus.Pending;

    public Guid? SettlementId { get; set; }
    public SharedSettlement? Settlement { get; set; }
}
