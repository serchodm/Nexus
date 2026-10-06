using Nexus.Domain.Common;

namespace Nexus.Domain.Entities;

public class ConnectedBankAccount : BaseEntity
{
    public Guid BankConnectionId { get; set; }
    public BankConnection BankConnection { get; set; } = null!;

    // Identificador único de cuenta en la API del ASPSP / Enable Banking
    public string ExternalAccountId { get; set; } = string.Empty;
    public string? Iban { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string Currency { get; set; } = "EUR";

    // Saldos reportados por la entidad
    public decimal? CurrentBalance { get; set; }
    public decimal? AvailableBalance { get; set; }
    public DateTimeOffset? BalanceUpdatedAt { get; set; }

    // Enlace opcional a la cuenta contable interna en Nexus
    public Guid? InternalBankAccountId { get; set; }
    public BankAccount? InternalBankAccount { get; set; }
}
