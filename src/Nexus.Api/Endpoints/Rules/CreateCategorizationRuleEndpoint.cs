using System.Security.Claims;
using FastEndpoints;
using Nexus.Application.Rules.Commands.CreateCategorizationRule;
using Nexus.Application.Rules.DTOs;
using Nexus.Domain.Common;
using Nexus.Domain.Enums;

namespace Nexus.Api.Endpoints.Rules;

public class CreateCategorizationRuleRequest
{
    public string Name { get; set; } = string.Empty;
    public string Pattern { get; set; } = string.Empty;
    public RuleMatchType MatchType { get; set; } = RuleMatchType.Contains;
    public Guid TargetCategoryId { get; set; }
    public Guid? TargetSubcategoryId { get; set; }
    public int Priority { get; set; } = 100;
    public ExpenseNature? SetExpenseNature { get; set; }
    public bool? SetIsRecurring { get; set; }
    public ExpenseFlexibility? SetExpenseFlexibility { get; set; }
    public bool ApplyRetroactive { get; set; } = false;
}

public sealed class CreateCategorizationRuleEndpoint : Endpoint<CreateCategorizationRuleRequest, CategorizationRuleDto>
{
    private readonly CreateCategorizationRuleCommandHandler _handler;

    public CreateCategorizationRuleEndpoint(CreateCategorizationRuleCommandHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Post("/rules");
        Summary(s =>
        {
            s.Summary = "Crea una nueva regla de categorización automática";
            s.Description = "Permite al usuario automatizar la asignación de categorías para comercios o patrones textuales, con opción de aplicación retroactiva inmediata.";
        });
    }

    public override async Task HandleAsync(CreateCategorizationRuleRequest req, CancellationToken ct)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(sub, out var userId))
        {
            ThrowError("Usuario no autenticado.", 401);
        }

        try
        {
            var cmd = new CreateCategorizationRuleCommand(
                UserId: userId,
                Name: req.Name,
                Pattern: req.Pattern,
                MatchType: req.MatchType,
                TargetCategoryId: req.TargetCategoryId,
                TargetSubcategoryId: req.TargetSubcategoryId,
                Priority: req.Priority,
                SetExpenseNature: req.SetExpenseNature,
                SetIsRecurring: req.SetIsRecurring,
                SetExpenseFlexibility: req.SetExpenseFlexibility,
                ApplyRetroactive: req.ApplyRetroactive
            );

            var result = await _handler.HandleAsync(cmd, ct);
            await Send.CreatedAtAsync<GetRulesEndpoint>(null, result, cancellation: ct);
        }
        catch (DomainValidationException ex)
        {
            ThrowError(ex.Message, 400);
        }
    }
}
