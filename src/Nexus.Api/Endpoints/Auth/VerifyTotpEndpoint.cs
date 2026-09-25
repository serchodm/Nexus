using System.Security.Claims;
using FastEndpoints;
using FluentValidation;
using Nexus.Application.Auth.Commands.VerifyTotp;
using Nexus.Application.Auth.DTOs;

namespace Nexus.Api.Endpoints.Auth;

public record VerifyTotpRequest(
    string Code,
    string? MfaToken
);

public record VerifyTotpResponse(
    bool Success,
    string? AccessToken,
    UserProfileDto? User
);

public sealed class VerifyTotpValidator : Validator<VerifyTotpRequest>
{
    public VerifyTotpValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("El código de 6 dígitos es requerido.")
            .Length(6).WithMessage("El código debe tener exactamente 6 dígitos.")
            .Matches(@"^\d{6}$").WithMessage("El código solo puede contener dígitos numéricos.");
    }
}

public sealed class VerifyTotpEndpoint : Endpoint<VerifyTotpRequest, VerifyTotpResponse>
{
    private readonly VerifyTotpCommandHandler _handler;

    public VerifyTotpEndpoint(VerifyTotpCommandHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Post("/auth/mfa/verify");
        AllowAnonymous(); // Permite invocación con mfaToken en el login de 2 fases o con sesión activa para activación
        Summary(s =>
        {
            s.Summary = "Verifica el código de 6 dígitos de Google Authenticator";
            s.Description = "Confirma la activación de MFA o completa el inicio de sesión de 2 factores.";
        });
    }

    public override async Task HandleAsync(VerifyTotpRequest req, CancellationToken ct)
    {
        Guid? authenticatedUserId = null;
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (Guid.TryParse(sub, out var parsedId))
        {
            authenticatedUserId = parsedId;
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var command = new VerifyTotpCommand(
            req.MfaToken,
            req.Code,
            authenticatedUserId,
            ipAddress
        );

        var result = await _handler.HandleAsync(command, ct);

        if (!result.Success)
        {
            ThrowError(result.ErrorMessage ?? "Código de verificación inválido.", 400);
        }

        // Si el resultado emitió un Refresh Token (completó login de 2 fases), inyectar cookie
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

        await Send.OkAsync(new VerifyTotpResponse(
            result.Success,
            result.AccessToken,
            result.User
        ), ct);
    }
}
