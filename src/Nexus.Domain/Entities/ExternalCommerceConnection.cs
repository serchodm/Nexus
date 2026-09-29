using Nexus.Domain.Common;
using Nexus.Domain.Enums;

namespace Nexus.Domain.Entities;

public class ExternalCommerceConnection : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public CommerceProvider Provider { get; set; } = CommerceProvider.PayPal;
    public string AccountEmail { get; set; } = string.Empty;
    
    // Tokens cifrados con AES-256-GCM (RN-07)
    public string EncryptedAccessToken { get; set; } = string.Empty;
    public string? EncryptedRefreshToken { get; set; }
    public DateTime? TokenExpiresAt { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime? LastSyncAt { get; set; }
    public string? LastSyncStatus { get; set; }
}
