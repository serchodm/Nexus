using Nexus.Application.Common.Interfaces;
using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Nexus.Domain.Enums;
using Nexus.Domain.Repositories;

namespace Nexus.Application.Commerce.Commands;

public record SyncPayPalApiCommand(
    Guid UserId,
    DateTime? StartDate = null,
    DateTime? EndDate = null
);

public class SyncPayPalApiCommandHandler
{
    private readonly IExternalCommerceConnectionRepository _connectionRepository;
    private readonly IExternalCommerceOrderRepository _orderRepository;
    private readonly IPayPalApiClient _payPalApiClient;
    private readonly ISecretEncryptionService _encryptionService;
    private readonly RunReconciliationCommandHandler _reconciliationHandler;
    private readonly IUnitOfWork _unitOfWork;

    public SyncPayPalApiCommandHandler(
        IExternalCommerceConnectionRepository connectionRepository,
        IExternalCommerceOrderRepository orderRepository,
        IPayPalApiClient payPalApiClient,
        ISecretEncryptionService encryptionService,
        RunReconciliationCommandHandler reconciliationHandler,
        IUnitOfWork unitOfWork)
    {
        _connectionRepository = connectionRepository;
        _orderRepository = orderRepository;
        _payPalApiClient = payPalApiClient;
        _encryptionService = encryptionService;
        _reconciliationHandler = reconciliationHandler;
        _unitOfWork = unitOfWork;
    }

    public async Task<ImportCommerceReportResult> HandleAsync(SyncPayPalApiCommand request, CancellationToken cancellationToken = default)
    {
        var connection = await _connectionRepository.GetByUserIdAndProviderAsync(request.UserId, CommerceProvider.PayPal, cancellationToken);
        if (connection == null || !connection.IsActive)
        {
            throw new InvalidOperationException("No se encontró una conexión activa con PayPal para este usuario.");
        }

        var accessToken = _encryptionService.Decrypt(connection.EncryptedAccessToken);
        var start = request.StartDate ?? DateTime.UtcNow.AddDays(-30);
        var end = request.EndDate ?? DateTime.UtcNow;

        List<PayPalTransactionItem> rawItems;
        try
        {
            rawItems = await _payPalApiClient.GetTransactionsAsync(accessToken, start, end, cancellationToken);
            connection.LastSyncAt = DateTime.UtcNow;
            connection.LastSyncStatus = "Success";
        }
        catch (Exception ex)
        {
            connection.LastSyncAt = DateTime.UtcNow;
            connection.LastSyncStatus = $"Error: {ex.Message}";
            _connectionRepository.Update(connection);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            throw;
        }

        var newOrders = 0;
        var skipped = 0;
        decimal totalAmount = 0m;

        foreach (var item in rawItems)
        {
            // Solo sincronizamos pagos emitidos (GrossAmount < 0)
            if (item.GrossAmount >= 0)
            {
                continue;
            }

            var absAmount = Math.Abs(item.GrossAmount);
            var existing = await _orderRepository.GetByExternalIdAsync(
                request.UserId,
                CommerceProvider.PayPal,
                item.TransactionId,
                cancellationToken);

            if (existing != null)
            {
                skipped++;
                continue;
            }

            var cleanCounterparty = string.IsNullOrWhiteSpace(item.CounterpartyName) ? null : item.CounterpartyName.Trim();
            var itemTitle = string.IsNullOrWhiteSpace(item.ItemSubject)
                ? (cleanCounterparty ?? "Pago PayPal")
                : item.ItemSubject.Trim();

            var order = new ExternalCommerceOrder
            {
                UserId = request.UserId,
                Provider = CommerceProvider.PayPal,
                ExternalOrderId = item.TransactionId,
                OrderDate = item.TransactionDate,
                TotalAmount = absAmount,
                Currency = string.IsNullOrWhiteSpace(item.Currency) ? "EUR" : item.Currency.ToUpperInvariant(),
                CounterpartyName = cleanCounterparty,
                CreatedAt = DateTime.UtcNow
            };

            order.Items.Add(new ExternalCommerceOrderItem
            {
                ExternalCommerceOrderId = order.Id,
                Sku = item.TransactionId,
                Title = itemTitle,
                Quantity = 1,
                UnitPrice = absAmount,
                TotalPrice = absAmount,
                CreatedAt = DateTime.UtcNow
            });

            await _orderRepository.AddAsync(order, cancellationToken);
            newOrders++;
            totalAmount += absAmount;
        }

        _connectionRepository.Update(connection);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (newOrders > 0)
        {
            await _reconciliationHandler.HandleAsync(new RunReconciliationCommand(request.UserId), cancellationToken);
        }

        return new ImportCommerceReportResult(
            rawItems.Count,
            newOrders,
            skipped,
            totalAmount
        );
    }
}
