using FastEndpoints;
using FluentValidation;
using Nexus.Application.Auth.Commands.GoogleLogin;
using Nexus.Application.Auth.DTOs;

namespace Nexus.Api.Endpoints.Auth;

public record GoogleLoginRequest(string IdToken);

public record GoogleLoginResponse(
    bool MfaRequired,
    string? MfaToken,
    string? AccessToken,
    string? RefreshToken,
    UserProfileDto? User
);

public sealed class GoogleLoginValidator : Validator<GoogleLoginRequest>
{
    public GoogleLoginValidator()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty().WithMessage("El token de Google es requerido.");
    }
}

public sealed class GoogleLoginEndpoint : Endpoint<GoogleLoginRequest, GoogleLoginResponse>
{
    private readonly GoogleLoginCommandHandler _handler;

    public GoogleLoginEndpoint(GoogleLoginCommandHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Post("/auth/google");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Inicia sesión o registra usuario mediante Google OAuth";
            s.Description = "Valida el ID Token de Google (o 'dev-mock:email' en desarrollo), emite Access Token en JSON y Refresh Token en Cookie HttpOnly.";
        });
    }

    public override async Task HandleAsync(GoogleLoginRequest req, CancellationToken ct)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var command = new GoogleLoginCommand(req.IdToken, ipAddress);

        var result = await _handler.HandleAsync(command, ct);

        if (!result.Success)
        {
            ThrowError(result.ErrorMessage ?? "Error al autenticar con Google.", 400);
        }

        // Si se emitió Refresh Token, inyectar la Cookie HttpOnly
        if (!string.IsNullOrWhiteSpace(result.RawRefreshToken))
        {
            HttpContext.Response.Cookies.Append("nexus_refresh_token", result.RawRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = HttpContext.Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(14),
                Path = "/api/auth"
            });
        }

        var isMobile = HttpContext.Request.Headers.TryGetValue("X-Client-Platform", out var platform) 
            && (platform.ToString().Equals("mobile", StringComparison.OrdinalIgnoreCase)
                || platform.ToString().Equals("android", StringComparison.OrdinalIgnoreCase)
                || platform.ToString().Equals("ios", StringComparison.OrdinalIgnoreCase));

        await Send.OkAsync(new GoogleLoginResponse(
            result.MfaRequired,
            result.MfaToken,
            result.AccessToken,
            isMobile ? result.RawRefreshToken : null,
            result.User
        ), ct);
    }
}
