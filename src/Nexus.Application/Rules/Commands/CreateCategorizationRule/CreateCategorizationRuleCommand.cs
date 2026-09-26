using Nexus.Application.Rules.Commands.ApplyRulesRetroactive;
using Nexus.Application.Rules.DTOs;
using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Nexus.Domain.Enums;
using Nexus.Domain.Repositories;

namespace Nexus.Application.Rules.Commands.CreateCategorizationRule;

public record CreateCategorizationRuleCommand(
    Guid UserId,
    string Name,
    string Pattern,
    RuleMatchType MatchType,
    Guid TargetCategoryId,
    Guid? TargetSubcategoryId,
    int Priority = 100,
    ExpenseNature? SetExpenseNature = null,
    bool? SetIsRecurring = null,
    ExpenseFlexibility? SetExpenseFlexibility = null,
    bool ApplyRetroactive = false
);

public class CreateCategorizationRuleCommandHandler
{
    private readonly ICategorizationRuleRepository _ruleRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplyRulesRetroactiveCommandHandler _retroactiveHandler;

    public CreateCategorizationRuleCommandHandler(
        ICategorizationRuleRepository ruleRepository,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        ApplyRulesRetroactiveCommandHandler retroactiveHandler)
    {
        _ruleRepository = ruleRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _retroactiveHandler = retroactiveHandler;
    }

    public async Task<CategorizationRuleDto> HandleAsync(CreateCategorizationRuleCommand cmd, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(cmd.Pattern))
        {
            throw new DomainValidationException("El patrón de búsqueda es obligatorio.");
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

        var ruleName = string.IsNullOrWhiteSpace(cmd.Name)
            ? $"Regla: {cmd.Pattern.Trim()} -> {targetCategory.Name}"
            : cmd.Name.Trim();

        var rule = new CategorizationRule
        {
            UserId = cmd.UserId,
            Name = ruleName,
            Pattern = cmd.Pattern.Trim(),
            NormalizedPattern = cmd.Pattern.Trim().ToUpperInvariant(),
            MatchType = cmd.MatchType,
            TargetCategoryId = cmd.TargetCategoryId,
            TargetSubcategoryId = cmd.TargetSubcategoryId,
            Priority = cmd.Priority,
            SetExpenseNature = cmd.SetExpenseNature,
            SetIsRecurring = cmd.SetIsRecurring,
            SetExpenseFlexibility = cmd.SetExpenseFlexibility,
            IsActive = true
        };

        await _ruleRepository.AddAsync(rule, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        if (cmd.ApplyRetroactive)
        {
            await _retroactiveHandler.HandleAsync(new ApplyRulesRetroactiveCommand(cmd.UserId), ct);
        }

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
