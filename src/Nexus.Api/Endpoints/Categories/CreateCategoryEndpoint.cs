using System.Security.Claims;
using FastEndpoints;
using Nexus.Application.Categories.Commands.CreateCategory;
using Nexus.Application.Categories.DTOs;
using Nexus.Domain.Common;
using Nexus.Domain.Enums;

namespace Nexus.Api.Endpoints.Categories;

public class CreateCategoryRequest
{
    public Guid? ParentCategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = "tag";
    public string ColorHex { get; set; } = "#64748B";
    public FlowType FlowType { get; set; } = FlowType.Expense;
    public ExpenseNature DefaultExpenseNature { get; set; } = ExpenseNature.Individual;
    public int DisplayOrder { get; set; } = 0;
}

public sealed class CreateCategoryEndpoint : Endpoint<CreateCategoryRequest, CategoryDto>
{
    private readonly CreateCategoryCommandHandler _handler;

    public CreateCategoryEndpoint(CreateCategoryCommandHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Post("/categories");
        Summary(s =>
        {
            s.Summary = "Crea una categoría o subcategoría personalizada";
            s.Description = "Permite al usuario definir nuevas categorías con colores e iconos HSL.";
        });
    }

    public override async Task HandleAsync(CreateCategoryRequest req, CancellationToken ct)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(sub, out var userId))
        {
            ThrowError("Usuario no autenticado.", 401);
        }

        try
        {
            var cmd = new CreateCategoryCommand(
                UserId: userId,
                ParentCategoryId: req.ParentCategoryId,
                Name: req.Name,
                Icon: req.Icon,
                ColorHex: req.ColorHex,
                FlowType: req.FlowType,
                DefaultExpenseNature: req.DefaultExpenseNature,
                DisplayOrder: req.DisplayOrder
            );

            var result = await _handler.HandleAsync(cmd, ct);
            await Send.CreatedAtAsync<GetCategoryTreeEndpoint>(null, result, cancellation: ct);
        }
        catch (DomainValidationException ex)
        {
            ThrowError(ex.Message, 400);
        }
    }
}
