using System.Security.Claims;
using FastEndpoints;
using Nexus.Application.Categories.DTOs;
using Nexus.Application.Categories.Queries.GetCategoryTree;

namespace Nexus.Api.Endpoints.Categories;

public sealed class GetCategoryTreeEndpoint : EndpointWithoutRequest<List<CategoryTreeDto>>
{
    private readonly GetCategoryTreeQueryHandler _handler;

    public GetCategoryTreeEndpoint(GetCategoryTreeQueryHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Get("/categories");
        Summary(s =>
        {
            s.Summary = "Obtiene el árbol jerárquico de categorías";
            s.Description = "Devuelve las categorías padre y sus subcategorías (predeterminadas del sistema más personalizadas del usuario).";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(sub, out var userId))
        {
            ThrowError("Usuario no autenticado.", 401);
        }

        var result = await _handler.HandleAsync(new GetCategoryTreeQuery(userId), ct);
        await Send.OkAsync(result, ct);
    }
}
