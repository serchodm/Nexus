using Nexus.Domain.Entities;

namespace Nexus.Application.Common.Interfaces;

public interface IJwtProvider
{
    string GenerateAccessToken(User user);
    string GenerateMfaPendingToken(User user);
    Guid? ValidateMfaPendingToken(string token);
}
