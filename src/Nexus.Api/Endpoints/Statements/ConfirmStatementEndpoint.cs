using System.Security.Claims;
using FastEndpoints;
using FluentValidation;
using Nexus.Application.Statements.Commands.ConfirmStatementImport;
using Nexus.Application.Statements.DTOs;

namespace Nexus.Api.Endpoints.Statements;

public class ConfirmStatementRequest
{
    public Guid Id { get; set; }
    public Guid PreviewToken { get; set; }
}

public sealed class ConfirmStatementValidator : Validator<ConfirmStatementRequest>
{
    public ConfirmStatementValidator()
    {
        RuleFor(x => x.PreviewToken)
            .NotEmpty().WithMessage("El token de previsualización (previewToken) es requerido.");
    }
}

public sealed class ConfirmStatementEndpoint : Endpoint<ConfirmStatementRequest, ImportSummaryDto>
{
    private readonly ConfirmStatementImportCommandHandler _handler;

    public ConfirmStatementEndpoint(ConfirmStatementImportCommandHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Post("/accounts/{id}/statements/confirm");
        Summary(s =>
        {
            s.Summary = "Confirma e importa un extracto previsualizado";
            s.Description = "Persiste el extracto canónico mediante el previewToken efímero de 15 minutos, deduplicando por fingerprint SHA-256 y actualizando el saldo.";
        });
    }

    public override async Task HandleAsync(ConfirmStatementRequest req, CancellationToken ct)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(sub, out var userId))
        {
            ThrowError("Usuario no autenticado.", 401);
        }

        var command = new ConfirmStatementImportCommand(
            UserId: userId,
            BankAccountId: req.Id,
            PreviewToken: req.PreviewToken
        );

        var result = await _handler.HandleAsync(command, ct);

        if (!result.Success || result.Summary == null)
        {
            ThrowError(result.ErrorMessage ?? "Error al confirmar la importación del extracto.", 400);
        }

        await Send.OkAsync(result.Summary, ct);
    }
}
