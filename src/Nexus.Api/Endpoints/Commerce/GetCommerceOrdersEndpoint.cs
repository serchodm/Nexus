using System.Security.Claims;
using FastEndpoints;
using Nexus.Application.Commerce.Queries;
using Nexus.Domain.Enums;

namespace Nexus.Api.Endpoints.Commerce;

public class GetCommerceOrdersRequest
{
    public CommerceProvider? Provider { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public sealed class GetCommerceOrdersEndpoint : Endpoint<GetCommerceOrdersRequest, List<CommerceOrderDto>>
{
    private readonly GetCommerceOrdersQueryHandler _handler;

    public GetCommerceOrdersEndpoint(GetCommerceOrdersQueryHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Get("/commerce/orders");
        Summary(s =>
        {
            s.Summary = "Obtiene el listado de pedidos externos importados de Amazon y PayPal";
            s.Description = "Permite paginar y filtrar por proveedor de comercio para auditar pedidos y artículos asociados.";
        });
    }

    public override async Task HandleAsync(GetCommerceOrdersRequest req, CancellationToken ct)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(sub, out var userId))
        {
            ThrowError("Usuario no autenticado.", 401);
        }

        var query = new GetCommerceOrdersQuery(userId, req.Provider, req.Page, req.PageSize);
        var result = await _handler.HandleAsync(query, ct);

        await Send.OkAsync(result, ct);
    }
}
