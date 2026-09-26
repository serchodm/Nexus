using Nexus.Application.Statements.DTOs;

namespace Nexus.Application.Statements.Queries.GetBankAccounts;

public record GetBankAccountsQuery(Guid UserId);

public record GetBankAccountsResult(
    List<BankAccountDto> Accounts,
    decimal TotalNetWorthShare
);
