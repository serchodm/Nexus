namespace Nexus.Application.Common.Interfaces;

public interface ITotpService
{
    string GenerateSecretKey();
    string GenerateQrCodeUri(string email, string secretKey);
    bool VerifyCode(string secretKey, string code, long? lastStep, out long matchedStep);
}
