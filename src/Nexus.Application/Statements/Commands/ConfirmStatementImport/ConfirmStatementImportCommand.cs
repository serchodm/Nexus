using Nexus.Application.Statements.DTOs;

namespace Nexus.Application.Statements.Commands.ConfirmStatementImport;

public record ConfirmStatementImportCommand(
    Guid UserId,
    Guid BankAccountId,
    Guid PreviewToken
);

public record ConfirmStatementImportResult(
    bool Success,
    ImportSummaryDto? Summary = null,
    string? ErrorMessage = null
);
