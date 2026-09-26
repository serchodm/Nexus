using Nexus.Domain.Enums;

namespace Nexus.Application.Statements.Parsers;

public record StatementParseResult(
    BankType BankType,
    List<ParsedRawTransaction> Transactions,
    decimal StartingBalance,
    decimal EndingBalance,
    DateOnly StartDate,
    DateOnly EndDate,
    List<string> DetectedCards
);
