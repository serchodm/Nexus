using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nexus.Application.Common.Interfaces;

namespace Nexus.Infrastructure.Services;

public sealed class GoogleAuthService : IGoogleAuthService
{
    private readonly string? _clientId;
    private readonly ILogger<GoogleAuthService> _logger;

    public GoogleAuthService(IConfiguration configuration, ILogger<GoogleAuthService> logger)
    {
        _clientId = configuration["Google:ClientId"];
        _logger = logger;
    }

    public async Task<GoogleUserPayload?> ValidateTokenAsync(string idToken, CancellationToken cancellationToken = default)
    {
        // 1. Soporte para Modo Desarrollo / Mock (para desarrollo ágil sin credenciales de Google Cloud)
        if (idToken.StartsWith("dev-mock:", StringComparison.OrdinalIgnoreCase))
        {
            var email = idToken["dev-mock:".Length..].Trim();
            if (string.IsNullOrWhiteSpace(email))
            {
                email = "developer@nexus.local";
            }

            _logger.LogInformation("Usando autenticación simulada (Dev Mock) para email: {Email}", email);

            var deterministicSub = $"google-sub-mock-{Math.Abs(email.GetHashCode()):x8}";
            var name = email.Split('@')[0];
            name = char.ToUpperInvariant(name[0]) + name[1..];

            return new GoogleUserPayload(
                SubjectId: deterministicSub,
                Email: email.ToLowerInvariant(),
                FullName: $"{name} (Dev)",
                AvatarUrl: $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(name)}&background=0D8ABC&color=fff"
            );
        }

        // 2. Validación Criptográfica Real de Google OAuth
        if (string.IsNullOrWhiteSpace(_clientId))
        {
            _logger.LogWarning("No se ha configurado 'Google:ClientId'. Rechazando token real.");
            return null;
        }

        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [_clientId]
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
            if (payload == null) return null;

            return new GoogleUserPayload(
                SubjectId: payload.Subject,
                Email: payload.Email.ToLowerInvariant(),
                FullName: payload.Name ?? payload.Email,
                AvatarUrl: payload.Picture
            );
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogWarning(ex, "Token de Google inválido.");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al validar token de Google.");
            return null;
        }
    }
}
