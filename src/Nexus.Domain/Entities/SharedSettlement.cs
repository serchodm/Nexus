using Nexus.Domain.Common;
using Nexus.Domain.Enums;

namespace Nexus.Domain.Entities;

/// <summary>
/// Registro contable de liquidación y extinción de deuda en un espacio compartido.
/// </summary>
public class SharedSettlement : BaseEntity
{
    public Guid SharedSpaceId { get; set; }
    public SharedSpace SharedSpace { get; set; } = null!;

    public Guid PayerMemberId { get; set; }
    public SharedSpaceMember PayerMember { get; set; } = null!;

    public Guid ReceiverMemberId { get; set; }
    public SharedSpaceMember ReceiverMember { get; set; } = null!;

    public decimal Amount { get; set; }
    public SettlementMethod Method { get; set; } = SettlementMethod.Bizum;
    public string? ReferenceCode { get; set; }
    public DateTimeOffset SettledAt { get; set; } = DateTimeOffset.UtcNow;
    
    // Vínculo opcional a transacción bancaria de abono conciliado
    public Guid? LinkedTransactionId { get; set; }
    public BankTransaction? LinkedTransaction { get; set; }

    public ICollection<SharedExpenseEntry> SettledExpenses { get; set; } = new List<SharedExpenseEntry>();
}
