namespace Nexus.Application.Auth.DTOs;

public record UserProfileDto(
    Guid Id,
    string Email,
    string FullName,
    string? AvatarUrl,
    string Role,
    bool IsMfaEnabled
);
