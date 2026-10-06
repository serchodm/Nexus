using System.Security.Claims;
using FastEndpoints;
using Nexus.Application.Banking.Commands;
using Nexus.Application.Banking.DTOs;

namespace Nexus.Api.Endpoints.Banking;

public record StartBankAuthSessionRequest(
    string InstitutionCode,
    string? RedirectUri
);

public sealed class StartBankAuthSessionEndpoint : Endpoint<StartBankAuthSessionRequest, BankAuthSessionDto>
{
    private readonly StartBankAuthSessionCommandHandler _handler;

    public StartBankAuthSessionEndpoint(StartBankAuthSessionCommandHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Post("/banking/auth-session");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Inicia una sesión de autorización Open Banking SCA con una entidad bancaria";
            s.Description = "Genera la URL de redirección bancaria y registra la conexión en estado PendingAuthorization.";
        });
    }

    public override async Task HandleAsync(StartBankAuthSessionRequest req, CancellationToken ct)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(sub, out var userId))
        {
            userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        }

        var command = new StartBankAuthSessionCommand(userId, req.InstitutionCode, req.RedirectUri);
        var result = await _handler.HandleAsync(command, ct);

        await Send.OkAsync(result, ct);
    }
}
