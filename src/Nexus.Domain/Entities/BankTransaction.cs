using Nexus.Domain.Common;
using Nexus.Domain.Contracts;

namespace Nexus.Domain.Entities;

public class BankTransaction : BaseEntity, IBankTransaction
{
    public Guid BankAccountId { get; set; }
    public BankAccount BankAccount { get; set; } = null!;

    public Guid StatementImportId { get; set; }
    public StatementImport StatementImport { get; set; } = null!;

    public DateOnly OperationDate { get; set; }
    public DateOnly? ValueDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EUR";
    public decimal? BalanceAfter { get; set; }

    // Categorización nativa del banco (preservada de ING Direct, etc.)
    public string? BankCategory { get; set; }
    public string? BankSubcategory { get; set; }
    public string? BankMovementType { get; set; }
    public string? Observations { get; set; }

    // Inmutabilidad y Auditoría: Fila original exacta en JSON
    public string RawDataJson { get; set; } = "{}";

    // Deduplicación determinista: SHA256(AccountId + Date + Amount + NormalizedDesc + BalanceAfter)
    public string Fingerprint { get; set; } = string.Empty;
}
