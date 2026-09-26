using Nexus.Domain.Common;

namespace Nexus.Domain.Contracts;

public interface IBankTransaction : IEntity, IAuditableEntity
{
    Guid BankAccountId { get; set; }
    Guid StatementImportId { get; set; }
    DateOnly OperationDate { get; set; }
    DateOnly? ValueDate { get; set; }
    string Description { get; set; }
    decimal Amount { get; set; }
    string Currency { get; set; }
    decimal? BalanceAfter { get; set; }
    string? BankCategory { get; set; }
    string? BankSubcategory { get; set; }
    string? BankMovementType { get; set; }
    string? Observations { get; set; }
    string RawDataJson { get; set; }
    string Fingerprint { get; set; }
}
