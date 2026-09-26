using Nexus.Domain.Common;
using Nexus.Domain.Contracts;

namespace Nexus.Domain.Entities;

public class User : BaseEntity, IUser
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? GoogleSubjectId { get; set; }
    public string Role { get; set; } = "User";
    public bool IsActive { get; set; } = true;

    // MFA / TOTP (RFC 6238)
    public bool IsMfaEnabled { get; set; }
    public string? MfaSecret { get; set; }
    public long? LastTotpStep { get; set; }

    // Navegación
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<CategorizationRule> CategorizationRules { get; set; } = new List<CategorizationRule>();
}
