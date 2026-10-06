using System.Security.Claims;
using FastEndpoints;
using Nexus.Application.SharedExpenses.DTOs;
using Nexus.Application.SharedExpenses.Queries;

namespace Nexus.Api.Endpoints.SharedSpaces;

public sealed class GetActiveSharedSpaceEndpoint : EndpointWithoutRequest<SharedSpaceSummaryDto>
{
    private readonly GetActiveSharedSpaceQueryHandler _handler;

    public GetActiveSharedSpaceEndpoint(GetActiveSharedSpaceQueryHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Get("/shared-spaces/active");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Obtiene el espacio compartido activo del usuario";
            s.Description = "Devuelve los miembros, configuración de reparto, balance neto bilateral y partidas pendientes.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(sub, out var userId))
        {
            userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        }

        var result = await _handler.HandleAsync(new GetActiveSharedSpaceQuery(userId), ct);
        if (result == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(result, ct);
    }
}
