using System.Security.Claims;
using FastEndpoints;
using Nexus.Application.Categories.Commands.UpdateCategory;
using Nexus.Application.Categories.DTOs;
using Nexus.Domain.Common;
using Nexus.Domain.Enums;

namespace Nexus.Api.Endpoints.Categories;

public class UpdateCategoryRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = "tag";
    public string ColorHex { get; set; } = "#64748B";
    public ExpenseNature DefaultExpenseNature { get; set; } = ExpenseNature.Individual;
    public int DisplayOrder { get; set; } = 0;
}

public sealed class UpdateCategoryEndpoint : Endpoint<UpdateCategoryRequest, CategoryDto>
{
    private readonly UpdateCategoryCommandHandler _handler;

    public UpdateCategoryEndpoint(UpdateCategoryCommandHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Put("/categories/{id}");
        Summary(s =>
        {
            s.Summary = "Actualiza una categoría personalizada";
            s.Description = "Modifica atributos visuales o de imputación de una categoría propia del usuario.";
        });
    }

    public override async Task HandleAsync(UpdateCategoryRequest req, CancellationToken ct)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(sub, out var userId))
        {
            ThrowError("Usuario no autenticado.", 401);
        }

        try
        {
            var cmd = new UpdateCategoryCommand(
                Id: req.Id,
                UserId: userId,
                Name: req.Name,
                Icon: req.Icon,
                ColorHex: req.ColorHex,
                DefaultExpenseNature: req.DefaultExpenseNature,
                DisplayOrder: req.DisplayOrder
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
