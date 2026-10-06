using System.Security.Claims;
using FastEndpoints;
using Nexus.Application.Banking.Commands;
using Nexus.Application.Banking.DTOs;

namespace Nexus.Api.Endpoints.Banking;

public record CompleteBankAuthCallbackRequest(
    string Code,
    string SessionId
);

public sealed class CompleteBankAuthCallbackEndpoint : Endpoint<CompleteBankAuthCallbackRequest, BankConnectionDto>
{
    private readonly CompleteBankAuthCallbackCommandHandler _handler;

    public CompleteBankAuthCallbackEndpoint(CompleteBankAuthCallbackCommandHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Post("/banking/callback");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Finaliza el callback de autorización tras la redirección bancaria";
            s.Description = "Intercambia el código de autorización, activa el consentimiento de 180 días y descubre las cuentas.";
        });
    }

    public override async Task HandleAsync(CompleteBankAuthCallbackRequest req, CancellationToken ct)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(sub, out var userId))
        {
            userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        }

        var command = new CompleteBankAuthCallbackCommand(userId, req.Code, req.SessionId);
        var result = await _handler.HandleAsync(command, ct);

        if (result == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(result, ct);
    }
}
