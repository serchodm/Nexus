using Nexus.Domain.Common;
using Nexus.Domain.Contracts;
using Nexus.Domain.Enums;

namespace Nexus.Domain.Entities;

public class BankAccount : BaseEntity, IBankAccount
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public BankType BankType { get; set; } = BankType.Generic;
    public string BankName { get; set; } = string.Empty;
    public string? BankCode { get; set; }
    public string? Iban { get; set; }
    public string Last4Digits { get; set; } = string.Empty;
    public string Currency { get; set; } = "EUR";

    // Clasificación Patrimonial
    public AccountType Type { get; set; } = AccountType.Checking;
    public HolderRole HolderRole { get; set; } = HolderRole.Owner;
    public decimal OwnershipPercentage { get; set; } = 100m;

    // Saldo Consolidado Actual
    public decimal? CurrentBalance { get; set; }
    public DateTimeOffset? BalanceLastUpdatedAt { get; set; }

    // Tarjetas vinculadas para detección cruzada en extractos
    public List<string> LinkedCardsLast4 { get; set; } = new();

    // Relaciones
    public ICollection<StatementImport> Imports { get; set; } = new List<StatementImport>();
    public ICollection<BankTransaction> Transactions { get; set; } = new List<BankTransaction>();
}
