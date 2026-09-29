using Nexus.Domain.Common;

namespace Nexus.Domain.Entities;

public class ExternalCommerceOrderItem : BaseEntity
{
    public Guid ExternalCommerceOrderId { get; set; }
    public ExternalCommerceOrder Order { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string? Sku { get; set; }               // ASIN en Amazon
    public string? CategoryHint { get; set; }      // Categoría sugerida por Amazon
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string? Condition { get; set; }

    // Seguimiento de reconciliación (RN-04: 1 ítem solo puede vincularse a un cargo bancario)
    public Guid? BankTransactionId { get; set; }
    public BankTransaction? BankTransaction { get; set; }
    public bool IsReconciled => BankTransactionId.HasValue;
}
