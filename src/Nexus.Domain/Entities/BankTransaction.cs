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

    // --- Enriquecimiento Feature 002: Categorización e Inteligencia Financiera ---
    public string? CleanMerchantName { get; set; }

    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }

    public Guid? SubcategoryId { get; set; }
    public Category? Subcategory { get; set; }

    public Enums.CategorizationSource CategorizationSource { get; set; } = Enums.CategorizationSource.None;
    public bool IsManual { get; set; } = false; // RN-01: Inmutable contra reglas si es true

    public bool IsRecurring { get; set; } = false;
    public Enums.RecurrencePeriod RecurrencePeriod { get; set; } = Enums.RecurrencePeriod.None;
    public Enums.ExpenseFlexibility ExpenseFlexibility { get; set; } = Enums.ExpenseFlexibility.Discretionary;
    public Enums.ExpenseNature ExpenseNature { get; set; } = Enums.ExpenseNature.Individual;

    public Enums.ExternalPlatform ExternalPlatform { get; set; } = Enums.ExternalPlatform.None;
    public bool PendingPlatformEnrichment { get; set; } = false;

    public bool HasSplits { get; set; } = false;
    public ICollection<TransactionSplitItem> SplitItems { get; set; } = new List<TransactionSplitItem>();
}
