using Nexus.Domain.Common;
using Nexus.Domain.Enums;

namespace Nexus.Domain.Entities;

/// <summary>
/// Regla determinista de asignación automática de categorías y atributos contables.
/// </summary>
public class CategorizationRule : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string Pattern { get; set; } = string.Empty;
    public string NormalizedPattern { get; set; } = string.Empty;
    public RuleMatchType MatchType { get; set; } = RuleMatchType.Contains;

    public Guid TargetCategoryId { get; set; }
    public Category TargetCategory { get; set; } = null!;

    public Guid? TargetSubcategoryId { get; set; }
    public Category? TargetSubcategory { get; set; }

    /// <summary>
    /// Prioridad de evaluación (mayor número = mayor precedencia).
    /// </summary>
    public int Priority { get; set; } = 100;

    /// <summary>
    /// Enriquecimientos opcionales fijados por la regla.
    /// </summary>
    public ExpenseNature? SetExpenseNature { get; set; }
    public bool? SetIsRecurring { get; set; }
    public ExpenseFlexibility? SetExpenseFlexibility { get; set; }

    public bool IsActive { get; set; } = true;
}
