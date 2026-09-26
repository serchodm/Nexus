using Nexus.Domain.Enums;

namespace Nexus.Application.Statements.DTOs;

public record StatementPreviewRowDto(
    DateOnly OperationDate,
    string Description,
    decimal Amount,
    decimal? BalanceAfter,
    string? BankCategory
);

public record StatementPreviewDto(
    Guid PreviewToken,
    BankType DetectedBankType,
    string DetectedBankName,
    decimal StartingBalance,
    decimal EndingBalance,
    DateOnly StartDate,
    DateOnly EndDate,
    int TotalRecords,
    decimal TotalIncome,
    decimal TotalExpense,
    List<string> Warnings,
    List<StatementPreviewRowDto> PreviewRows
);

public record ImportSummaryDto(
    Guid ImportId,
    Guid BankAccountId,
    string FileName,
    int TotalRecords,
    int ImportedRecords,
    int SkippedDuplicates,
    decimal FinalAccountBalance
);
