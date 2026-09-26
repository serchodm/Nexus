using System.Security.Claims;
using FastEndpoints;
using Nexus.Application.Rules.DTOs;
using Nexus.Application.Rules.Queries.GetRules;

namespace Nexus.Api.Endpoints.Rules;

public sealed class GetRulesEndpoint : EndpointWithoutRequest<List<CategorizationRuleDto>>
{
    private readonly GetRulesQueryHandler _handler;

    public GetRulesEndpoint(GetRulesQueryHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Get("/rules");
        Summary(s =>
        {
            s.Summary = "Obtiene las reglas de categorización del usuario";
            s.Description = "Lista las reglas de auto-categorización ordenadas por prioridad de ejecución descendente.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(sub, out var userId))
        {
            ThrowError("Usuario no autenticado.", 401);
        }

        var result = await _handler.HandleAsync(new GetRulesQuery(userId), ct);
        await Send.OkAsync(result, ct);
    }
}
