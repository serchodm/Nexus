using Nexus.Domain.Common;
using Nexus.Domain.Contracts;
using Nexus.Domain.Enums;

namespace Nexus.Domain.Entities;

public class StatementImport : BaseEntity, IStatementImport
{
    public Guid BankAccountId { get; set; }
    public BankAccount BankAccount { get; set; } = null!;

    public string FileName { get; set; } = string.Empty;
    public string FileHashSha256 { get; set; } = string.Empty;
    public BankType DetectedBankType { get; set; } = BankType.Generic;

    public DateTimeOffset ImportedAt { get; set; } = DateTimeOffset.UtcNow;
    public int TotalRecords { get; set; }
    public int ImportedRecords { get; set; }
    public int SkippedDuplicates { get; set; }

    public decimal StartingBalance { get; set; }
    public decimal EndingBalance { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public ICollection<BankTransaction> Transactions { get; set; } = new List<BankTransaction>();
}
