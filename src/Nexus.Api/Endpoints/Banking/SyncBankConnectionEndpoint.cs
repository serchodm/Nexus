using System.Security.Claims;
using FastEndpoints;
using Nexus.Application.Banking.Commands;
using Nexus.Application.Banking.DTOs;

namespace Nexus.Api.Endpoints.Banking;

public record SyncBankConnectionRequest
{
    public Guid Id { get; init; }
    public bool? Force { get; init; }
}

public sealed class SyncBankConnectionEndpoint : Endpoint<SyncBankConnectionRequest, SyncResultDto>
{
    private readonly SyncBankConnectionCommandHandler _handler;

    public SyncBankConnectionEndpoint(SyncBankConnectionCommandHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Post("/banking/connections/{id}/sync");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Sincroniza bajo demanda una conexión bancaria agregada";
            s.Description = "Descarga saldos actualizados y movimientos recientes, deduplicando contra el libro mayor.";
        });
    }

    public override async Task HandleAsync(SyncBankConnectionRequest req, CancellationToken ct)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(sub, out var userId))
        {
            userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        }

        var command = new SyncBankConnectionCommand(userId, req.Id, req.Force ?? false);
        var result = await _handler.HandleAsync(command, ct);

        if (result == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(result, ct);
    }
}
