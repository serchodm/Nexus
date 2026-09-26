using Nexus.Application.Statements.DTOs;

namespace Nexus.Application.Statements.Queries.GetAccountTransactions;

public record GetAccountTransactionsQuery(
    Guid UserId,
    Guid AccountId,
    int Page = 1,
    int PageSize = 50,
    DateOnly? FromDate = null,
    DateOnly? ToDate = null
);

public record GetAccountTransactionsResult(
    List<BankTransactionDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
