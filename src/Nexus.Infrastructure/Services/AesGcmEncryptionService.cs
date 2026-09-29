using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Nexus.Application.Common.Interfaces;

namespace Nexus.Infrastructure.Services;

public sealed class AesGcmEncryptionService : ISecretEncryptionService
{
    private readonly byte[] _key;

    public AesGcmEncryptionService(IConfiguration configuration)
    {
        var rawKey = configuration["Security:EncryptionKey"] ?? "nexus-super-secret-encryption-key-2026-financial-suite";
        _key = SHA256.HashData(Encoding.UTF8.GetBytes(rawKey));
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return string.Empty;

        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var nonce = new byte[AesGcm.NonceByteSizes.MaxSize]; // 12 bytes
        RandomNumberGenerator.Fill(nonce);

        var cipherBytes = new byte[plainBytes.Length];
        var tag = new byte[AesGcm.TagByteSizes.MaxSize]; // 16 bytes

        using var aesGcm = new AesGcm(_key, AesGcm.TagByteSizes.MaxSize);
        aesGcm.Encrypt(nonce, plainBytes, cipherBytes, tag);

        // Formato empaquetado: [nonce (12)] + [tag (16)] + [ciphertext (N)]
        var result = new byte[nonce.Length + tag.Length + cipherBytes.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
        Buffer.BlockCopy(tag, 0, result, nonce.Length, tag.Length);
        Buffer.BlockCopy(cipherBytes, 0, result, nonce.Length + tag.Length, cipherBytes.Length);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return string.Empty;

        var fullBytes = Convert.FromBase64String(cipherText);
        var nonceLength = AesGcm.NonceByteSizes.MaxSize;
        var tagLength = AesGcm.TagByteSizes.MaxSize;

        if (fullBytes.Length < nonceLength + tagLength)
        {
            throw new CryptographicException("Cifrado inválido o longitud insuficiente.");
        }

        var nonce = new byte[nonceLength];
        var tag = new byte[tagLength];
        var cipherBytes = new byte[fullBytes.Length - nonceLength - tagLength];

        Buffer.BlockCopy(fullBytes, 0, nonce, 0, nonceLength);
        Buffer.BlockCopy(fullBytes, nonceLength, tag, 0, tagLength);
        Buffer.BlockCopy(fullBytes, nonceLength + tagLength, cipherBytes, 0, cipherBytes.Length);

        var plainBytes = new byte[cipherBytes.Length];
        using var aesGcm = new AesGcm(_key, tagLength);
        aesGcm.Decrypt(nonce, cipherBytes, tag, plainBytes);

        return Encoding.UTF8.GetString(plainBytes);
    }
}
