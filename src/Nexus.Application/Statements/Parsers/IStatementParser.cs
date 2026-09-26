using Nexus.Domain.Enums;

namespace Nexus.Application.Statements.Parsers;

public interface IStatementParser
{
    BankType HandledBankType { get; }
    bool CanParse(string headerLine);
    Task<StatementParseResult> ParseAsync(Stream fileStream, string fileName, CancellationToken ct = default);
}
