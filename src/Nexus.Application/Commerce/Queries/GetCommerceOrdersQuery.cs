using Nexus.Domain.Enums;
using Nexus.Domain.Repositories;

namespace Nexus.Application.Commerce.Queries;

public record CommerceOrderItemDto(
    Guid Id,
    string? Sku,
    string Title,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    Guid? BankTransactionId
);

public record CommerceOrderDto(
    Guid Id,
    CommerceProvider Provider,
    string ExternalOrderId,
    DateTime OrderDate,
    decimal TotalAmount,
    string Currency,
    string? CounterpartyName,
    bool IsFullyReconciled,
    List<CommerceOrderItemDto> Items
);

public record GetCommerceOrdersQuery(
    Guid UserId,
    CommerceProvider? Provider = null,
    int Page = 1,
    int PageSize = 50
);

public class GetCommerceOrdersQueryHandler
{
    private readonly IExternalCommerceOrderRepository _orderRepository;

    public GetCommerceOrdersQueryHandler(IExternalCommerceOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<List<CommerceOrderDto>> HandleAsync(GetCommerceOrdersQuery request, CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetOrdersPagedAsync(
            request.UserId,
            request.Provider,
            request.Page,
            request.PageSize,
            cancellationToken);

        return orders.Select(o => new CommerceOrderDto(
            o.Id,
            o.Provider,
            o.ExternalOrderId,
            o.OrderDate,
            o.TotalAmount,
            o.Currency,
            o.CounterpartyName,
            o.IsFullyReconciled,
            o.Items.Select(i => new CommerceOrderItemDto(
                i.Id,
                i.Sku,
                i.Title,
                i.Quantity,
                i.UnitPrice,
                i.TotalPrice,
                i.BankTransactionId
            )).ToList()
        )).ToList();
    }
}
