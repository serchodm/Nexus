using System.Security.Cryptography;
using System.Text;
using Nexus.Application.Common.Interfaces;

namespace Nexus.Infrastructure.Services;

public sealed class TokenGenerator : ITokenGenerator
{
    public (string RawToken, string TokenHash) GenerateRefreshToken()
    {
        // 32 bytes (256 bits) de entropía aleatoria criptográfica
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);

        var rawToken = Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');

        var tokenHash = HashToken(rawToken);

        return (rawToken, tokenHash);
    }

    public string HashToken(string rawToken)
    {
        var bytes = Encoding.UTF8.GetBytes(rawToken);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToHexStringLower(hashBytes);
    }
}
