using System.Security.Claims;
using FastEndpoints;
using Nexus.Application.Categories.Commands.DeleteCategory;
using Nexus.Domain.Common;

namespace Nexus.Api.Endpoints.Categories;

public class DeleteCategoryRequest
{
    public Guid Id { get; set; }
}

public sealed class DeleteCategoryEndpoint : Endpoint<DeleteCategoryRequest>
{
    private readonly DeleteCategoryCommandHandler _handler;

    public DeleteCategoryEndpoint(DeleteCategoryCommandHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Delete("/categories/{id}");
        Summary(s =>
        {
            s.Summary = "Elimina una categoría personalizada";
            s.Description = "Realiza un borrado lógico de una categoría de usuario.";
        });
    }

    public override async Task HandleAsync(DeleteCategoryRequest req, CancellationToken ct)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(sub, out var userId))
        {
            ThrowError("Usuario no autenticado.", 401);
        }

        try
        {
            await _handler.HandleAsync(new DeleteCategoryCommand(req.Id, userId), ct);
            await Send.NoContentAsync(ct);
        }
        catch (DomainValidationException ex)
        {
            ThrowError(ex.Message, 400);
        }
    }
}
