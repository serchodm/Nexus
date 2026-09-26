using Nexus.Application.Statements.DTOs;
using Nexus.Domain.Enums;

namespace Nexus.Application.Statements.Commands.CreateBankAccount;

public record CreateBankAccountCommand(
    Guid UserId,
    string Name,
    string? Iban,
    AccountType Type,
    HolderRole HolderRole,
    decimal? OwnershipPercentage,
    List<string>? LinkedCardsLast4
);

public record CreateBankAccountResult(
    bool Success,
    BankAccountDto? Account = null,
    string? ErrorMessage = null
);
