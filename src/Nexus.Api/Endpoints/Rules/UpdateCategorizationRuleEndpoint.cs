using System.Security.Claims;
using FastEndpoints;
using Nexus.Application.Rules.Commands.UpdateCategorizationRule;
using Nexus.Application.Rules.DTOs;
using Nexus.Domain.Common;
using Nexus.Domain.Enums;

namespace Nexus.Api.Endpoints.Rules;

public class UpdateCategorizationRuleRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Pattern { get; set; } = string.Empty;
    public RuleMatchType MatchType { get; set; } = RuleMatchType.Contains;
    public Guid TargetCategoryId { get; set; }
    public Guid? TargetSubcategoryId { get; set; }
    public int Priority { get; set; } = 100;
    public ExpenseNature? SetExpenseNature { get; set; }
    public bool? SetIsRecurring { get; set; }
    public ExpenseFlexibility? SetExpenseFlexibility { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class UpdateCategorizationRuleEndpoint : Endpoint<UpdateCategorizationRuleRequest, CategorizationRuleDto>
{
    private readonly UpdateCategorizationRuleCommandHandler _handler;

    public UpdateCategorizationRuleEndpoint(UpdateCategorizationRuleCommandHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Put("/rules/{id}");
        Summary(s =>
        {
            s.Summary = "Modifica una regla de categorización existente";
            s.Description = "Permite cambiar el patrón de texto, categoría de destino o prioridad de una regla.";
        });
    }

    public override async Task HandleAsync(UpdateCategorizationRuleRequest req, CancellationToken ct)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(sub, out var userId))
        {
            ThrowError("Usuario no autenticado.", 401);
        }

        try
        {
            var cmd = new UpdateCategorizationRuleCommand(
                Id: req.Id,
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
                IsActive: req.IsActive
            );

            var result = await _handler.HandleAsync(cmd, ct);
            await Send.OkAsync(result, ct);
        }
        catch (DomainValidationException ex)
        {
            ThrowError(ex.Message, 400);
        }
    }
}
