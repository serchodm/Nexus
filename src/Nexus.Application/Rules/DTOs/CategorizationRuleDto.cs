using Nexus.Domain.Enums;

namespace Nexus.Application.Rules.DTOs;

public record CategorizationRuleDto(
    Guid Id,
    string Name,
    string Pattern,
    RuleMatchType MatchType,
    Guid TargetCategoryId,
    string TargetCategoryName,
    Guid? TargetSubcategoryId,
    string? TargetSubcategoryName,
    int Priority,
    ExpenseNature? SetExpenseNature,
    bool? SetIsRecurring,
    ExpenseFlexibility? SetExpenseFlexibility,
    bool IsActive
);
