using Nexus.Domain.Common;
using Nexus.Domain.Enums;

namespace Nexus.Domain.Entities;

public class BankConnection : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string InstitutionCode { get; set; } = string.Empty;
    public string InstitutionName { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }

    // Identificador de sesión en Enable Banking
    public string? ExternalSessionId { get; set; }

    // Credenciales y tokens cifrados con AES-256-GCM
    public string? EncryptedAccessToken { get; set; }
    public string? EncryptedRefreshToken { get; set; }

    // Consentimiento SCA (180 días según EBA RTS)
    public DateTimeOffset? ConsentExpiresAt { get; set; }
    public DateTimeOffset? LastSyncedAt { get; set; }
    public BankConnectionStatus Status { get; set; } = BankConnectionStatus.PendingAuthorization;

    // Cuentas agregadas asociadas
    public ICollection<ConnectedBankAccount> Accounts { get; set; } = new List<ConnectedBankAccount>();
}
