namespace Nexus.Application.Statements.Parsers;

public record ParsedRawTransaction(
    DateOnly OperationDate,
    DateOnly? ValueDate,
    string Description,
    decimal Amount,
    string Currency,
    decimal? BalanceAfter,
    string? BankCategory,
    string? BankSubcategory,
    string? BankMovementType,
    string? Observations,
    string RawDataJson,
    string? DetectedCardLast4
);
