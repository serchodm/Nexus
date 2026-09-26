using System.Security.Cryptography;
using System.Text;
using Nexus.Domain.Security;

namespace Nexus.Domain.Services;

public static class TransactionFingerprintGenerator
{
    /// <summary>
    /// Genera un hash SHA-256 determinista para deduplicación de transacciones en la misma cuenta.
    /// </summary>
    public static string Generate(Guid accountId, DateOnly operationDate, decimal amount, string description, decimal? balanceAfter)
    {
        var normalizedDesc = TextSanitizer.NormalizeForFingerprint(description);
        var rawKey = $"{accountId:N}|{operationDate:O}|{amount:F2}|{normalizedDesc}|{balanceAfter:F2}";
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawKey));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
