using Nexus.Domain.Common;
using Nexus.Domain.Enums;

namespace Nexus.Domain.Contracts;

public interface IStatementImport : IEntity, IAuditableEntity
{
    Guid BankAccountId { get; set; }
    string FileName { get; set; }
    string FileHashSha256 { get; set; }
    BankType DetectedBankType { get; set; }
    DateTimeOffset ImportedAt { get; set; }
    int TotalRecords { get; set; }
    int ImportedRecords { get; set; }
    int SkippedDuplicates { get; set; }
    decimal StartingBalance { get; set; }
    decimal EndingBalance { get; set; }
    DateOnly StartDate { get; set; }
    DateOnly EndDate { get; set; }
}
