using System.Security.Claims;
using FastEndpoints;
using Nexus.Application.Commerce.Commands;

namespace Nexus.Api.Endpoints.Commerce;

public class ConnectPayPalRequest
{
    public string AccountEmail { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public int? ExpiresInSeconds { get; set; }
}

public sealed class ConnectPayPalEndpoint : Endpoint<ConnectPayPalRequest, ConnectPayPalResult>
{
    private readonly ConnectPayPalCommandHandler _handler;

    public ConnectPayPalEndpoint(ConnectPayPalCommandHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Post("/commerce/paypal/connect");
        Summary(s =>
        {
            s.Summary = "Vincula credenciales de API / OAuth2 de PayPal";
            s.Description = "Almacena los tokens cifrados con AES-256-GCM en reposo y habilita la sincronización programada o bajo demanda.";
        });
    }

    public override async Task HandleAsync(ConnectPayPalRequest req, CancellationToken ct)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(sub, out var userId))
        {
            ThrowError("Usuario no autenticado.", 401);
        }

        var command = new ConnectPayPalCommand(
            userId,
            req.AccountEmail,
            req.AccessToken,
            req.RefreshToken,
            req.ExpiresInSeconds
        );

        var result = await _handler.HandleAsync(command, ct);
        if (!result.Success)
        {
            ThrowError(result.Message ?? "Error al conectar la cuenta de PayPal.", 400);
        }

        await Send.OkAsync(result, ct);
    }
}
