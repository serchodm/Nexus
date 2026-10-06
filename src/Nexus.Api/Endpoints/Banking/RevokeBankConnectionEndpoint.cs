using System.Security.Claims;
using FastEndpoints;
using Nexus.Application.Banking.Commands;

namespace Nexus.Api.Endpoints.Banking;

public record RevokeBankConnectionRequest
{
    public Guid Id { get; init; }
}

public sealed class RevokeBankConnectionEndpoint : Endpoint<RevokeBankConnectionRequest>
{
    private readonly RevokeBankConnectionCommandHandler _handler;

    public RevokeBankConnectionEndpoint(RevokeBankConnectionCommandHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Delete("/banking/connections/{id}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Revoca y desconecta una entidad bancaria agregada";
            s.Description = "Marca el estado como Revocado y elimina credenciales y tokens criptográficos.";
        });
    }

    public override async Task HandleAsync(RevokeBankConnectionRequest req, CancellationToken ct)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(sub, out var userId))
        {
            userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        }

        var command = new RevokeBankConnectionCommand(userId, req.Id);
        var success = await _handler.HandleAsync(command, ct);

        if (!success)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.NoContentAsync(ct);
    }
}
