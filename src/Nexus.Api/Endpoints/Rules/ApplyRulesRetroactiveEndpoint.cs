using System.Security.Claims;
using FastEndpoints;
using Nexus.Application.Rules.Commands.ApplyRulesRetroactive;

namespace Nexus.Api.Endpoints.Rules;

public sealed class ApplyRulesRetroactiveEndpoint : EndpointWithoutRequest<ApplyRulesRetroactiveResult>
{
    private readonly ApplyRulesRetroactiveCommandHandler _handler;

    public ApplyRulesRetroactiveEndpoint(ApplyRulesRetroactiveCommandHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Post("/rules/apply-retroactive");
        Summary(s =>
        {
            s.Summary = "Aplica retroactivamente todas las reglas de categorización";
            s.Description = "Re-evalúa todas las transacciones históricas no manuales contra las reglas activas del usuario respetando RN-01.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(sub, out var userId))
        {
            ThrowError("Usuario no autenticado.", 401);
        }

        var result = await _handler.HandleAsync(new ApplyRulesRetroactiveCommand(userId), ct);
        await Send.OkAsync(result, ct);
    }
}
