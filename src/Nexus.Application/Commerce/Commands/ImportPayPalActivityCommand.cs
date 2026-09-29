using Nexus.Application.Commerce.Parsers;
using Nexus.Domain.Common;
using Nexus.Domain.Enums;
using Nexus.Domain.Repositories;

namespace Nexus.Application.Commerce.Commands;

public record ImportPayPalActivityCommand(
    Guid UserId,
    Stream CsvStream
);

public class ImportPayPalActivityCommandHandler
{
    private readonly IExternalCommerceOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly RunReconciliationCommandHandler _reconciliationHandler;
    private readonly PayPalStatementCsvParser _parser = new();

    public ImportPayPalActivityCommandHandler(
        IExternalCommerceOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        RunReconciliationCommandHandler reconciliationHandler)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _reconciliationHandler = reconciliationHandler;
    }

    public async Task<ImportCommerceReportResult> HandleAsync(ImportPayPalActivityCommand request, CancellationToken cancellationToken = default)
    {
        var parsedOrders = await _parser.ParseAsync(request.CsvStream, request.UserId, cancellationToken);
        var newOrdersCount = 0;
        var skipped = 0;
        decimal totalAmount = 0m;

        foreach (var order in parsedOrders)
        {
            var existing = await _orderRepository.GetByExternalIdAsync(
                request.UserId,
                CommerceProvider.PayPal,
                order.ExternalOrderId,
                cancellationToken);

            if (existing != null)
            {
                skipped++;
                continue;
            }

            await _orderRepository.AddAsync(order, cancellationToken);
            newOrdersCount++;
            totalAmount += order.TotalAmount;
        }

        if (newOrdersCount > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            // Reconciliación automática post-importación
            await _reconciliationHandler.HandleAsync(new RunReconciliationCommand(request.UserId), cancellationToken);
        }

        return new ImportCommerceReportResult(
            parsedOrders.Count,
            newOrdersCount,
            skipped,
            totalAmount
        );
    }
}
