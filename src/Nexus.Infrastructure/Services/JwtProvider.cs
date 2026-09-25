using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Nexus.Application.Common.Interfaces;
using Nexus.Domain.Entities;

namespace Nexus.Infrastructure.Services;

public sealed class JwtProvider : IJwtProvider
{
    private readonly string _key;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expiryMinutes;

    public JwtProvider(IConfiguration configuration)
    {
        _key = configuration["Jwt:Key"] 
            ?? throw new InvalidOperationException("Falta la configuración de 'Jwt:Key'.");
        _issuer = configuration["Jwt:Issuer"] ?? "NexusApi";
        _audience = configuration["Jwt:Audience"] ?? "NexusFrontend";
        _expiryMinutes = int.TryParse(configuration["Jwt:ExpiryMinutes"], out var exp) ? exp : 15;
    }

    public string GenerateAccessToken(User user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Name, user.FullName),
            new(ClaimTypes.Role, user.Role),
            new("mfa_enabled", user.IsMfaEnabled.ToString().ToLowerInvariant())
        };

        return CreateToken(claims, TimeSpan.FromMinutes(_expiryMinutes));
    }

    public string GenerateMfaPendingToken(User user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new("scope", "mfa_pending")
        };

        // Token temporal con validez estricta de 5 minutos
        return CreateToken(claims, TimeSpan.FromMinutes(5));
    }

    public Guid? ValidateMfaPendingToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key)),
            ValidateIssuer = true,
            ValidIssuer = _issuer,
            ValidateAudience = true,
            ValidAudience = _audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            var scopeClaim = principal.FindFirst("scope")?.Value;
            if (scopeClaim != "mfa_pending")
            {
                return null;
            }

            var subClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value 
                ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (Guid.TryParse(subClaim, out var userId))
            {
                return userId;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private string CreateToken(IEnumerable<Claim> claims, TimeSpan expiration)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(expiration),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
