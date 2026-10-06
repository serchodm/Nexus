using System.Security.Claims;
using FastEndpoints;
using Nexus.Application.Banking.DTOs;
using Nexus.Application.Banking.Queries;

namespace Nexus.Api.Endpoints.Banking;

public sealed class GetBankConnectionsEndpoint : EndpointWithoutRequest<List<BankConnectionDto>>
{
    private readonly GetBankConnectionsQueryHandler _handler;

    public GetBankConnectionsEndpoint(GetBankConnectionsQueryHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Get("/banking/connections");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Obtiene las conexiones bancarias activas del usuario";
            s.Description = "Lista las entidades conectadas, cuentas agregadas, días de consentimiento restantes y último sync.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(sub, out var userId))
        {
            userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        }

        var connections = await _handler.HandleAsync(new GetBankConnectionsQuery(userId), ct);
        await Send.OkAsync(connections, ct);
    }
}
