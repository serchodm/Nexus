using Nexus.Domain.Common;

namespace Nexus.Domain.Contracts;

public interface IRefreshToken : IEntity, IAuditableEntity, ISoftDeletable
{
    Guid UserId { get; }
    string TokenHash { get; }
    DateTimeOffset ExpiresAt { get; }
    DateTimeOffset? RevokedAt { get; }
    string? ReplacedByTokenHash { get; }
    string? CreatedByIp { get; }
    bool IsExpired { get; }
    bool IsRevoked { get; }
    bool IsActive { get; }
}
