using System.Security.Claims;
using FastEndpoints;
using Nexus.Application.Commerce.Queries;

namespace Nexus.Api.Endpoints.Commerce;

public sealed class GetPendingReconciliationsEndpoint : EndpointWithoutRequest<List<ReconciliationCandidateDto>>
{
    private readonly GetPendingReconciliationsQueryHandler _handler;

    public GetPendingReconciliationsEndpoint(GetPendingReconciliationsQueryHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Get("/commerce/reconciliations");
        Summary(s =>
        {
            s.Summary = "Obtiene las propuestas de reconciliación pendientes de revisión";
            s.Description = "Devuelve los candidatos emparejados por el motor entre cargos bancarios opacos y pedidos de Amazon o PayPal.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(sub, out var userId))
        {
            ThrowError("Usuario no autenticado.", 401);
        }

        var result = await _handler.HandleAsync(new GetPendingReconciliationsQuery(userId), ct);
        await Send.OkAsync(result, ct);
    }
}
