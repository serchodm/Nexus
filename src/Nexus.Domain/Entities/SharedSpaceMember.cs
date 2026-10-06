using Nexus.Domain.Common;
using Nexus.Domain.Enums;

namespace Nexus.Domain.Entities;

/// <summary>
/// Miembro participante en un espacio compartido.
/// </summary>
public class SharedSpaceMember : BaseEntity
{
    public Guid SharedSpaceId { get; set; }
    public SharedSpace SharedSpace { get; set; } = null!;

    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public string DisplayName { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
    public string AvatarColor { get; set; } = "var(--primary)";
    public SharedSpaceRole Role { get; set; } = SharedSpaceRole.Member;
    
    /// <summary>
    /// Ingreso neto mensual declarado voluntariamente para el cálculo ponderado Fair-Share.
    /// </summary>
    public decimal? DeclaredMonthlyIncome { get; set; }
    public bool IsActive { get; set; } = true;
}
