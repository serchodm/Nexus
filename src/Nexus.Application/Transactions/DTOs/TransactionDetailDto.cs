using Nexus.Domain.Enums;

namespace Nexus.Application.Transactions.DTOs;

public record SplitItemDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    Guid? SubcategoryId,
    string? SubcategoryName,
    decimal Amount,
    string? Description,
    ExpenseNature ExpenseNature
);

public record SplitItemInput(
    Guid CategoryId,
    Guid? SubcategoryId,
    decimal Amount,
    string? Description,
    ExpenseNature ExpenseNature
);

public record TransactionDetailDto(
    Guid Id,
    Guid BankAccountId,
    DateOnly OperationDate,
    DateOnly? ValueDate,
    string Description,
    string? CleanMerchantName,
    decimal Amount,
    string Currency,
    decimal? BalanceAfter,
    Guid? CategoryId,
    string? CategoryName,
    string? CategoryColor,
    string? CategoryIcon,
    Guid? SubcategoryId,
    string? SubcategoryName,
    CategorizationSource CategorizationSource,
    bool IsManual,
    bool IsRecurring,
    RecurrencePeriod RecurrencePeriod,
    ExpenseFlexibility ExpenseFlexibility,
    ExpenseNature ExpenseNature,
    ExternalPlatform ExternalPlatform,
    bool PendingPlatformEnrichment,
    bool HasSplits,
    List<SplitItemDto> SplitItems
);

public record TransactionsPagedResult(
    List<TransactionDetailDto> Items,
    int TotalCount,
    int Page,
    int PageSize
);
