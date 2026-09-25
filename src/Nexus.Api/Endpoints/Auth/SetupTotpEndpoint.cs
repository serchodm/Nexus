using System.Security.Claims;
using FastEndpoints;
using Nexus.Application.Auth.Commands.SetupTotp;

namespace Nexus.Api.Endpoints.Auth;

public record SetupTotpResponse(
    string SecretKey,
    string QrCodeUri
);

public sealed class SetupTotpEndpoint : EndpointWithoutRequest<SetupTotpResponse>
{
    private readonly SetupTotpCommandHandler _handler;

    public SetupTotpEndpoint(SetupTotpCommandHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Post("/auth/mfa/setup");
        Summary(s =>
        {
            s.Summary = "Inicia el enrolamiento de MFA con Google Authenticator";
            s.Description = "Genera una clave Base32 provisional y la URI para el código QR.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(sub, out var userId))
        {
            ThrowError("Usuario no autenticado o identificador inválido.", 401);
        }

        var result = await _handler.HandleAsync(new SetupTotpCommand(userId), ct);

        if (!result.Success || result.SecretKey == null || result.QrCodeUri == null)
        {
            ThrowError(result.ErrorMessage ?? "Error al generar la configuración TOTP.", 400);
        }

        await Send.OkAsync(new SetupTotpResponse(result.SecretKey, result.QrCodeUri), ct);
    }
}
