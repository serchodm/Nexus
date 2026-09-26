using Nexus.Application.Statements.Parsers;

namespace Nexus.Application.Statements.Models;

public record CachedPreviewData(
    Guid PreviewToken,
    Guid BankAccountId,
    Guid UserId,
    string FileName,
    string FileHashSha256,
    StatementParseResult ParseResult
);
