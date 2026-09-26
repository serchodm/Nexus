using Nexus.Application.Statements.DTOs;

namespace Nexus.Application.Statements.Commands.PreviewStatement;

public record PreviewStatementCommand(
    Guid UserId,
    Guid BankAccountId,
    string FileName,
    Stream FileStream
);

public record PreviewStatementResult(
    bool Success,
    StatementPreviewDto? Preview = null,
    string? ErrorMessage = null
);
