using Nexus.Domain.Common;
using Nexus.Domain.Enums;

namespace Nexus.Domain.Entities;

public class ExternalCommerceOrder : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public CommerceProvider Provider { get; set; }
    public string ExternalOrderId { get; set; } = string.Empty; // ej. Amazon Order ID: 402-1234567-8901234
    public DateTime OrderDate { get; set; }
    public DateTime? ShipmentDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal RefundAmount { get; set; }
    public string Currency { get; set; } = "EUR";
    public string? CounterpartyName { get; set; } // Nombre del comercio en PayPal (ej. "Spotify", "Steam")
    public string? RawDataJson { get; set; }
    public bool IsFullyReconciled { get; set; }

    // Relaciones
    public ICollection<ExternalCommerceOrderItem> Items { get; set; } = new List<ExternalCommerceOrderItem>();
    public ICollection<TransactionReconciliation> Reconciliations { get; set; } = new List<TransactionReconciliation>();
}
