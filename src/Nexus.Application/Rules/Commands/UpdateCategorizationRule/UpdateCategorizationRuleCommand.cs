using Nexus.Application.Rules.DTOs;
using Nexus.Domain.Common;
using Nexus.Domain.Enums;
using Nexus.Domain.Repositories;

namespace Nexus.Application.Rules.Commands.UpdateCategorizationRule;

public record UpdateCategorizationRuleCommand(
    Guid Id,
    Guid UserId,
    string Name,
    string Pattern,
    RuleMatchType MatchType,
    Guid TargetCategoryId,
    Guid? TargetSubcategoryId,
    int Priority,
    ExpenseNature? SetExpenseNature,
    bool? SetIsRecurring,
    ExpenseFlexibility? SetExpenseFlexibility,
    bool IsActive
);

public class UpdateCategorizationRuleCommandHandler
{
    private readonly ICategorizationRuleRepository _ruleRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCategorizationRuleCommandHandler(
        ICategorizationRuleRepository ruleRepository,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _ruleRepository = ruleRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CategorizationRuleDto> HandleAsync(UpdateCategorizationRuleCommand cmd, CancellationToken ct = default)
    {
        var rule = await _ruleRepository.GetByIdAsync(cmd.Id, ct);
        if (rule == null || rule.UserId != cmd.UserId)
        {
            throw new DomainValidationException("La regla no existe o no tienes acceso a ella.");
        }

        var targetCategory = await _categoryRepository.GetByIdAsync(cmd.TargetCategoryId, ct);
        if (targetCategory == null)
        {
            throw new DomainValidationException("La categoría de destino no existe.");
        }

        string? targetSubcategoryName = null;
        if (cmd.TargetSubcategoryId.HasValue)
        {
            var targetSubcategory = await _categoryRepository.GetByIdAsync(cmd.TargetSubcategoryId.Value, ct);
            if (targetSubcategory == null)
            {
                throw new DomainValidationException("La subcategoría de destino no existe.");
            }
            targetSubcategoryName = targetSubcategory.Name;
        }

        rule.Name = cmd.Name.Trim();
        rule.Pattern = cmd.Pattern.Trim();
        rule.NormalizedPattern = cmd.Pattern.Trim().ToUpperInvariant();
        rule.MatchType = cmd.MatchType;
        rule.TargetCategoryId = cmd.TargetCategoryId;
        rule.TargetSubcategoryId = cmd.TargetSubcategoryId;
        rule.Priority = cmd.Priority;
        rule.SetExpenseNature = cmd.SetExpenseNature;
        rule.SetIsRecurring = cmd.SetIsRecurring;
        rule.SetExpenseFlexibility = cmd.SetExpenseFlexibility;
        rule.IsActive = cmd.IsActive;

        _ruleRepository.Update(rule);
        await _unitOfWork.SaveChangesAsync(ct);

        return new CategorizationRuleDto(
            rule.Id,
            rule.Name,
            rule.Pattern,
            rule.MatchType,
            rule.TargetCategoryId,
            targetCategory.Name,
            rule.TargetSubcategoryId,
            targetSubcategoryName,
            rule.Priority,
            rule.SetExpenseNature,
            rule.SetIsRecurring,
            rule.SetExpenseFlexibility,
            rule.IsActive
        );
    }
}
