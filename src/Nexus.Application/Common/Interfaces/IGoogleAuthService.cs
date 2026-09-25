namespace Nexus.Application.Common.Interfaces;

public record GoogleUserPayload(
    string SubjectId,
    string Email,
    string FullName,
    string? AvatarUrl
);

public interface IGoogleAuthService
{
    Task<GoogleUserPayload?> ValidateTokenAsync(string idToken, CancellationToken cancellationToken = default);
}
