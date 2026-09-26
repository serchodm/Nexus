using System.Security.Claims;
using FastEndpoints;
using Nexus.Application.Rules.Commands.DeleteCategorizationRule;
using Nexus.Domain.Common;

namespace Nexus.Api.Endpoints.Rules;

public class DeleteCategorizationRuleRequest
{
    public Guid Id { get; set; }
}

public sealed class DeleteCategorizationRuleEndpoint : Endpoint<DeleteCategorizationRuleRequest>
{
    private readonly DeleteCategorizationRuleCommandHandler _handler;

    public DeleteCategorizationRuleEndpoint(DeleteCategorizationRuleCommandHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Delete("/rules/{id}");
        Summary(s =>
        {
            s.Summary = "Elimina una regla de categorización";
            s.Description = "Realiza el borrado lógico de una regla propia del usuario.";
        });
    }

    public override async Task HandleAsync(DeleteCategorizationRuleRequest req, CancellationToken ct)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(sub, out var userId))
        {
            ThrowError("Usuario no autenticado.", 401);
        }

        try
        {
            await _handler.HandleAsync(new DeleteCategorizationRuleCommand(req.Id, userId), ct);
            await Send.NoContentAsync(ct);
        }
        catch (DomainValidationException ex)
        {
            ThrowError(ex.Message, 400);
        }
    }
}
