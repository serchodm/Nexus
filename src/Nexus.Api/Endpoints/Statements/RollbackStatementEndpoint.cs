using System.Security.Claims;
using FastEndpoints;
using Nexus.Application.Statements.Commands.RollbackStatementImport;

namespace Nexus.Api.Endpoints.Statements;

public class RollbackStatementRequest
{
    public Guid ImportId { get; set; }
}

public sealed class RollbackStatementEndpoint : Endpoint<RollbackStatementRequest>
{
    private readonly RollbackStatementImportCommandHandler _handler;

    public RollbackStatementEndpoint(RollbackStatementImportCommandHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Delete("/statements/{importId}");
        Summary(s =>
        {
            s.Summary = "Deshace en un clic una importación de extracto";
            s.Description = "Elimina en cascada el lote de importación y todas sus transacciones asociadas, recalculando el saldo actual de la cuenta.";
        });
    }

    public override async Task HandleAsync(RollbackStatementRequest req, CancellationToken ct)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(sub, out var userId))
        {
            ThrowError("Usuario no autenticado.", 401);
        }

        var result = await _handler.HandleAsync(new RollbackStatementImportCommand(userId, req.ImportId), ct);

        if (!result.Success)
        {
            ThrowError(result.ErrorMessage ?? "Error al deshacer la importación.", 400);
        }

        await Send.NoContentAsync(ct);
    }
}
