using System.Security.Claims;
using FastEndpoints;
using Nexus.Application.Commerce.Commands;

namespace Nexus.Api.Endpoints.Commerce;

public class AcceptReconciliationRequest
{
    public Guid Id { get; set; }
}

public sealed class AcceptReconciliationEndpoint : Endpoint<AcceptReconciliationRequest, AcceptReconciliationResult>
{
    private readonly AcceptReconciliationCommandHandler _handler;

    public AcceptReconciliationEndpoint(AcceptReconciliationCommandHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Post("/commerce/reconciliations/{id}/accept");
        Summary(s =>
        {
            s.Summary = "Acepta una propuesta de reconciliación";
            s.Description = "Aplica los artículos del pedido como TransactionSplitItems exactos (RN-02) y actualiza el nombre del comercio limpio (RN-06).";
        });
    }

    public override async Task HandleAsync(AcceptReconciliationRequest req, CancellationToken ct)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(sub, out var userId))
        {
            ThrowError("Usuario no autenticado.", 401);
        }

        var command = new AcceptReconciliationCommand(userId, req.Id);
        var result = await _handler.HandleAsync(command, ct);

        if (!result.Success)
        {
            ThrowError(result.Message ?? "Error al aceptar la reconciliación.", 400);
        }

        await Send.OkAsync(result, ct);
    }
}
