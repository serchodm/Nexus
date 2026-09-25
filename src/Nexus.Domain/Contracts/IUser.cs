using Nexus.Domain.Common;

namespace Nexus.Domain.Contracts;

public interface IUser : IEntity, IAuditableEntity, ISoftDeletable
{
    string Email { get; }
    string FullName { get; }
    string? AvatarUrl { get; }
    string? GoogleSubjectId { get; }
    string Role { get; }
    bool IsActive { get; }
    bool IsMfaEnabled { get; }
    string? MfaSecret { get; }
    long? LastTotpStep { get; }
}
