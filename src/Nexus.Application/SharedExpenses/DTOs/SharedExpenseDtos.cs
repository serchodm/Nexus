namespace Nexus.Application.SharedExpenses.DTOs;

public record SharedSpaceMemberDto(
    Guid Id,
    string DisplayName,
    string Initials,
    string AvatarColor,
    string Role,
    decimal? DeclaredMonthlyIncome
);

public record SharedExpenseDto(
    Guid Id,
    Guid SharedSpaceId,
    Guid? BankTransactionId,
    Guid? TransactionSplitItemId,
    string Description,
    DateTime TransactionDate,
    decimal TotalAmount,
    decimal PaidByShare,
    decimal OwedShare,
    Guid PaidByMemberId,
    string PaidByName,
    string CategoryName,
    string CategoryIcon,
    string Status
);

public record SharedSpaceSummaryDto(
    Guid Id,
    string Name,
    string DefaultSplitRatio,
    decimal NetBalance,
    decimal TotalSharedThisMonth,
    decimal UserPaidThisMonth,
    decimal PartnerPaidThisMonth,
    List<SharedSpaceMemberDto> Members,
    List<SharedExpenseDto> PendingExpenses
);

public record SharedSettlementDto(
    Guid Id,
    Guid SharedSpaceId,
    Guid PayerMemberId,
    string PayerName,
    Guid ReceiverMemberId,
    string ReceiverName,
    decimal Amount,
    string Method,
    string? ReferenceCode,
    DateTimeOffset SettledAt
);
