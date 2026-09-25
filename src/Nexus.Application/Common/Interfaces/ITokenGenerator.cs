namespace Nexus.Application.Common.Interfaces;

public interface ITokenGenerator
{
    (string RawToken, string TokenHash) GenerateRefreshToken();
    string HashToken(string rawToken);
}
