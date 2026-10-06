using Nexus.Domain.Common;
using Nexus.Domain.Enums;

namespace Nexus.Domain.Entities;

/// <summary>
/// Representa un espacio financiero colaborativo (ej. Pareja, Hogar, Piso Compartido).
/// </summary>
public class SharedSpace : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public SplitRuleType DefaultSplitRule { get; set; } = SplitRuleType.FixedRatio;
    public decimal DefaultOwnerPercentage { get; set; } = 55.00m;
    public decimal DefaultMemberPercentage { get; set; } = 45.00m;
    public string Currency { get; set; } = "EUR";

    public ICollection<SharedSpaceMember> Members { get; set; } = new List<SharedSpaceMember>();
    public ICollection<SharedExpenseEntry> Expenses { get; set; } = new List<SharedExpenseEntry>();
    public ICollection<SharedSettlement> Settlements { get; set; } = new List<SharedSettlement>();
}
