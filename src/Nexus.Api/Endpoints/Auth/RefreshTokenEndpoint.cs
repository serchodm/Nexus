using FastEndpoints;
using Nexus.Application.Auth.Commands.RefreshToken;
using Nexus.Application.Auth.DTOs;

namespace Nexus.Api.Endpoints.Auth;

public record RefreshTokenResponse(
    string AccessToken,
    UserProfileDto User
);

public sealed class RefreshTokenEndpoint : EndpointWithoutRequest<RefreshTokenResponse>
{
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenEndpoint(RefreshTokenCommandHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Post("/auth/refresh");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Renueva la sesión mediante la Cookie HttpOnly (Silent Refresh)";
            s.Description = "Aplica rotación estricta de Refresh Token y emite un nuevo Access Token.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (!HttpContext.Request.Cookies.TryGetValue("nexus_refresh_token", out var rawRefreshToken) 
            || string.IsNullOrWhiteSpace(rawRefreshToken))
        {
            ThrowError("No se encontró la cookie de sesión.", 401);
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var command = new RefreshTokenCommand(rawRefreshToken, ipAddress);

        var result = await _handler.HandleAsync(command, ct);

        if (!result.Success || result.AccessToken == null || result.User == null)
        {
            // Limpiar la cookie si falló la renovación
            HttpContext.Response.Cookies.Delete("nexus_refresh_token", new CookieOptions { Path = "/api/auth" });
            ThrowError(result.ErrorMessage ?? "No fue posible renovar la sesión.", 401);
        }

        // Inyectar el nuevo Refresh Token rotado en la Cookie HttpOnly
        if (!string.IsNullOrWhiteSpace(result.NewRawRefreshToken))
        {
            HttpContext.Response.Cookies.Append("nexus_refresh_token", result.NewRawRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = HttpContext.Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(14),
                Path = "/api/auth"
            });
        }

        await Send.OkAsync(new RefreshTokenResponse(result.AccessToken, result.User), ct);
    }
}
