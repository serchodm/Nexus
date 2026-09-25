using FastEndpoints;
using Nexus.Application.Auth.Commands.RefreshToken;
using Nexus.Application.Auth.DTOs;

namespace Nexus.Api.Endpoints.Auth;

public record RefreshTokenRequest(string? RefreshToken);

public record RefreshTokenResponse(
    string AccessToken,
    string? RefreshToken,
    UserProfileDto User
);

public sealed class RefreshTokenEndpoint : Endpoint<RefreshTokenRequest, RefreshTokenResponse>
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
            s.Summary = "Renueva la sesión mediante Cookie HttpOnly o JSON body (para apps móviles nativas)";
            s.Description = "Aplica rotación estricta de Refresh Token y emite un nuevo Access Token.";
        });
    }

    public override async Task HandleAsync(RefreshTokenRequest req, CancellationToken ct)
    {
        // 1. Obtener el token desde el cuerpo JSON (clientes móviles) o desde la Cookie HttpOnly (web)
        string? rawRefreshToken = req.RefreshToken;
        if (string.IsNullOrWhiteSpace(rawRefreshToken))
        {
            HttpContext.Request.Cookies.TryGetValue("nexus_refresh_token", out rawRefreshToken);
        }

        if (string.IsNullOrWhiteSpace(rawRefreshToken))
        {
            ThrowError("No se proporcionó el token de sesión (cookie o JSON body requerido).", 401);
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

        var isMobile = !string.IsNullOrWhiteSpace(req.RefreshToken) 
            || (HttpContext.Request.Headers.TryGetValue("X-Client-Platform", out var platform) 
                && (platform.ToString().Equals("mobile", StringComparison.OrdinalIgnoreCase)
                    || platform.ToString().Equals("android", StringComparison.OrdinalIgnoreCase)
                    || platform.ToString().Equals("ios", StringComparison.OrdinalIgnoreCase)));

        await Send.OkAsync(new RefreshTokenResponse(
            result.AccessToken,
            isMobile ? result.NewRawRefreshToken : null,
            result.User
        ), ct);
    }
}
