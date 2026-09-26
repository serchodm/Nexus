using Nexus.Application.Transactions.DTOs;
using Nexus.Domain.Entities;
using Nexus.Domain.Enums;
using Nexus.Domain.Repositories;

namespace Nexus.Application.Transactions.Queries.GetTransactions;

public record GetTransactionsQuery(
    Guid UserId,
    Guid? BankAccountId = null,
    Guid? CategoryId = null,
    Guid? SubcategoryId = null,
    string? SearchTerm = null,
    DateOnly? FromDate = null,
    DateOnly? ToDate = null,
    bool? IsRecurring = null,
    bool? HasSplits = null,
    ExpenseNature? ExpenseNature = null,
    int Page = 1,
    int PageSize = 50
);

public class GetTransactionsQueryHandler
{
    private readonly IBankTransactionRepository _transactionRepository;

    public GetTransactionsQueryHandler(IBankTransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<TransactionsPagedResult> HandleAsync(GetTransactionsQuery query, CancellationToken ct = default)
    {
        var (items, totalCount) = await _transactionRepository.SearchTransactionsAsync(
            query.UserId,
            query.BankAccountId,
            query.CategoryId,
            query.SubcategoryId,
            query.SearchTerm,
            query.FromDate,
            query.ToDate,
            query.IsRecurring,
            query.HasSplits,
            query.ExpenseNature,
            query.Page,
            query.PageSize,
            ct
        );

        var dtos = items.Select(MapToDetailDto).ToList();

        return new TransactionsPagedResult(
            dtos,
            totalCount,
            query.Page < 1 ? 1 : query.Page,
            query.PageSize is < 1 or > 200 ? 50 : query.PageSize
        );
    }

    public static TransactionDetailDto MapToDetailDto(BankTransaction tx)
    {
        return new TransactionDetailDto(
            tx.Id,
            tx.BankAccountId,
            tx.OperationDate,
            tx.ValueDate,
            tx.Description,
            tx.CleanMerchantName,
            tx.Amount,
            tx.Currency,
            tx.BalanceAfter,
            tx.CategoryId,
            tx.Category?.Name,
            tx.Category?.ColorHex,
            tx.Category?.Icon,
            tx.SubcategoryId,
            tx.Subcategory?.Name,
            tx.CategorizationSource,
            tx.IsManual,
            tx.IsRecurring,
            tx.RecurrencePeriod,
            tx.ExpenseFlexibility,
            tx.ExpenseNature,
            tx.ExternalPlatform,
            tx.PendingPlatformEnrichment,
            tx.HasSplits,
            tx.SplitItems.Select(s => new SplitItemDto(
                s.Id,
                s.CategoryId,
                s.Category?.Name ?? string.Empty,
                s.SubcategoryId,
                s.Subcategory?.Name,
                s.Amount,
                s.Description,
                s.ExpenseNature
            )).ToList()
        );
    }
}
