using System.Security.Claims;
using FastEndpoints;
using Nexus.Application.Statements.Commands.PreviewStatement;
using Nexus.Application.Statements.DTOs;

namespace Nexus.Api.Endpoints.Statements;

public class PreviewStatementRequest
{
    public Guid Id { get; set; }
    public IFormFile File { get; set; } = default!;
}

public sealed class PreviewStatementEndpoint : Endpoint<PreviewStatementRequest, StatementPreviewDto>
{
    private readonly PreviewStatementCommandHandler _handler;

    public PreviewStatementEndpoint(PreviewStatementCommandHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Post("/accounts/{id}/statements/preview");
        AllowFileUploads();
        Summary(s =>
        {
            s.Summary = "Previsualiza un extracto bancario";
            s.Description = "Parsea el fichero en memoria (0 escrituras en BD), evalúa discrepancias de saldo o tarjetas y emite un previewToken de 15 minutos.";
        });
    }

    public override async Task HandleAsync(PreviewStatementRequest req, CancellationToken ct)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(sub, out var userId))
        {
            ThrowError("Usuario no autenticado.", 401);
        }

        if (req.File == null || req.File.Length == 0)
        {
            ThrowError("No se proporcionó ningún archivo o el archivo está vacío.", 400);
        }

        using var stream = req.File.OpenReadStream();
        var command = new PreviewStatementCommand(
            UserId: userId,
            BankAccountId: req.Id,
            FileName: req.File.FileName,
            FileStream: stream
        );

        var result = await _handler.HandleAsync(command, ct);

        if (!result.Success || result.Preview == null)
        {
            ThrowError(result.ErrorMessage ?? "Error al previsualizar el extracto bancario.", 400);
        }

        await Send.OkAsync(result.Preview, ct);
    }
}
