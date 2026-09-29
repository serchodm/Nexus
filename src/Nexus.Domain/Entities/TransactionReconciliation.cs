using Nexus.Domain.Common;
using Nexus.Domain.Enums;

namespace Nexus.Domain.Entities;

public class TransactionReconciliation : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid BankTransactionId { get; set; }
    public BankTransaction BankTransaction { get; set; } = null!;

    public Guid ExternalCommerceOrderId { get; set; }
    public ExternalCommerceOrder ExternalCommerceOrder { get; set; } = null!;

    public ReconciliationStatus Status { get; set; } = ReconciliationStatus.Candidate;
    public MatchConfidence Confidence { get; set; }
    public string MatchReason { get; set; } = string.Empty;
    
    // IDs de ítems específicos de la orden que componen este cargo bancario
    public List<Guid> MatchedOrderItemIds { get; set; } = new();

    public DateTime ReconciledAt { get; set; } = DateTime.UtcNow;
    public bool AppliedAsSplits { get; set; }
}
