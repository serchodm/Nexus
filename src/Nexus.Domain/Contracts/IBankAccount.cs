using Nexus.Domain.Common;
using Nexus.Domain.Enums;

namespace Nexus.Domain.Contracts;

public interface IBankAccount : IEntity, IAuditableEntity, ISoftDeletable
{
    Guid UserId { get; set; }
    string Name { get; set; }
    BankType BankType { get; set; }
    string BankName { get; set; }
    string? BankCode { get; set; }
    string? Iban { get; set; }
    string Last4Digits { get; set; }
    string Currency { get; set; }
    AccountType Type { get; set; }
    HolderRole HolderRole { get; set; }
    decimal OwnershipPercentage { get; set; }
    decimal? CurrentBalance { get; set; }
    DateTimeOffset? BalanceLastUpdatedAt { get; set; }
    List<string> LinkedCardsLast4 { get; set; }
}
