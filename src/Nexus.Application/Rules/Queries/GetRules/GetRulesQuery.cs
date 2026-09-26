using Nexus.Application.Rules.DTOs;
using Nexus.Domain.Repositories;

namespace Nexus.Application.Rules.Queries.GetRules;

public record GetRulesQuery(Guid UserId);

public class GetRulesQueryHandler
{
    private readonly ICategorizationRuleRepository _ruleRepository;

    public GetRulesQueryHandler(ICategorizationRuleRepository ruleRepository)
    {
        _ruleRepository = ruleRepository;
    }

    public async Task<List<CategorizationRuleDto>> HandleAsync(GetRulesQuery query, CancellationToken ct = default)
    {
        var rules = await _ruleRepository.GetActiveRulesByUserIdAsync(query.UserId, ct);

        return rules.Select(r => new CategorizationRuleDto(
            r.Id,
            r.Name,
            r.Pattern,
            r.MatchType,
            r.TargetCategoryId,
            r.TargetCategory?.Name ?? string.Empty,
            r.TargetSubcategoryId,
            r.TargetSubcategory?.Name,
            r.Priority,
            r.SetExpenseNature,
            r.SetIsRecurring,
            r.SetExpenseFlexibility,
            r.IsActive
        )).ToList();
    }
}
