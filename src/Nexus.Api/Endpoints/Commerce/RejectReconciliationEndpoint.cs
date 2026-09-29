using System.Security.Claims;
using FastEndpoints;
using Nexus.Application.Commerce.Commands;

namespace Nexus.Api.Endpoints.Commerce;

public class RejectReconciliationRequest
{
    public Guid Id { get; set; }
    public string? Reason { get; set; }
}

public sealed class RejectReconciliationEndpoint : Endpoint<RejectReconciliationRequest, RejectReconciliationResult>
{
    private readonly RejectReconciliationCommandHandler _handler;

    public RejectReconciliationEndpoint(RejectReconciliationCommandHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Post("/commerce/reconciliations/{id}/reject");
        Summary(s =>
        {
            s.Summary = "Descarta una propuesta de reconciliación";
            s.Description = "Marca el candidato como descartado, impidiendo que vuelva a sugerirse automáticamente.";
        });
    }

    public override async Task HandleAsync(RejectReconciliationRequest req, CancellationToken ct)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(sub, out var userId))
        {
            ThrowError("Usuario no autenticado.", 401);
        }

        var command = new RejectReconciliationCommand(userId, req.Id, req.Reason);
        var result = await _handler.HandleAsync(command, ct);

        if (!result.Success)
        {
            ThrowError(result.Message ?? "Error al descartar la reconciliación.", 400);
        }

        await Send.OkAsync(result, ct);
    }
}
