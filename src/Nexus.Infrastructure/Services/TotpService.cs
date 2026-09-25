using Nexus.Application.Common.Interfaces;
using OtpNet;

namespace Nexus.Infrastructure.Services;

public sealed class TotpService : ITotpService
{
    private const int StepDurationSeconds = 30;
    private const int CodeDigits = 6;
    private const string IssuerName = "Nexus";

    public string GenerateSecretKey()
    {
        // Genera 160 bits (20 bytes) de entropía criptográfica para Base32
        var randomKey = KeyGeneration.GenerateRandomKey(20);
        return Base32Encoding.ToString(randomKey);
    }

    public string GenerateQrCodeUri(string email, string secretKey)
    {
        var encodedIssuer = Uri.EscapeDataString(IssuerName);
        var encodedEmail = Uri.EscapeDataString(email);

        return $"otpauth://totp/{encodedIssuer}:{encodedEmail}?secret={secretKey}&issuer={encodedIssuer}&algorithm=SHA1&digits={CodeDigits}&period={StepDurationSeconds}";
    }

    public bool VerifyCode(string secretKey, string code, long? lastStep, out long matchedStep)
    {
        matchedStep = 0;

        if (string.IsNullOrWhiteSpace(secretKey) || string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        try
        {
            var keyBytes = Base32Encoding.ToBytes(secretKey);
            var totp = new Totp(keyBytes, step: StepDurationSeconds, mode: OtpHashMode.Sha1, totpSize: CodeDigits);

            // Ventana de tolerancia: +/- 1 paso (30 segundos antes / después) para sincronización de reloj
            var window = new VerificationWindow(previous: 1, future: 1);
            var isValid = totp.VerifyTotp(code.Trim(), out matchedStep, window);

            if (!isValid)
            {
                return false;
            }

            // Protección contra ataques de repetición (Replay Attack):
            // El paso detectado debe ser estrictamente mayor que el último paso registrado
            if (lastStep.HasValue && matchedStep <= lastStep.Value)
            {
                return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }
}
