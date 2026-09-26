namespace Nexus.Application.Statements.Commands.RollbackStatementImport;

public record RollbackStatementImportCommand(
    Guid UserId,
    Guid ImportId
);

public record RollbackStatementImportResult(
    bool Success,
    string? ErrorMessage = null
);
