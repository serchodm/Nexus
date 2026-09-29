using Nexus.Domain.Entities;
using Nexus.Domain.Enums;

namespace Nexus.Domain.Services;

public record ReconciliationCandidate(
    Guid BankTransactionId,
    Guid ExternalCommerceOrderId,
    List<Guid> MatchedOrderItemIds,
    MatchConfidence Confidence,
    string MatchReason,
    string? SuggestedCleanMerchantName
);

public class CommerceReconciliationEngine
{
    public ReconciliationCandidate? FindBestMatch(
        BankTransaction bankTx,
        IReadOnlyList<ExternalCommerceOrder> candidateOrders)
    {
        if (bankTx.Amount >= 0) return null; // Solo reconciliamos gastos por ahora

        var targetAmount = Math.Abs(bankTx.Amount);
        var txDate = bankTx.OperationDate.ToDateTime(TimeOnly.MinValue);
        var upperDesc = bankTx.Description.ToUpperInvariant();

        // 1. Filtrar órdenes elegibles dentro de la ventana de -7 a +2 días
        var eligibleOrders = candidateOrders
            .Where(o => !o.IsFullyReconciled)
            .Where(o => o.OrderDate >= txDate.AddDays(-7) && o.OrderDate <= txDate.AddDays(2))
            .ToList();

        if (eligibleOrders.Count == 0) return null;

        // 2. Si el movimiento es de PayPal
        if (upperDesc.Contains("PAYPAL"))
        {
            var paypalOrders = eligibleOrders
                .Where(o => o.Provider == CommerceProvider.PayPal)
                .ToList();

            var paypalMatch = paypalOrders.FirstOrDefault(o => Math.Abs(o.TotalAmount - targetAmount) < 0.005m);
            if (paypalMatch != null)
            {
                var daysDiff = Math.Abs((txDate - paypalMatch.OrderDate).TotalDays);
                var confidence = daysDiff <= 2 ? MatchConfidence.Exact100 : MatchConfidence.High;
                var counterpartySuffix = !string.IsNullOrWhiteSpace(paypalMatch.CounterpartyName) ? $" ({paypalMatch.CounterpartyName})" : "";
                return new ReconciliationCandidate(
                    bankTx.Id,
                    paypalMatch.Id,
                    paypalMatch.Items.Select(i => i.Id).ToList(),
                    confidence,
                    $"Transacción PayPal emparejada{counterpartySuffix} por importe exacto ({targetAmount:F2} €) con {daysDiff:F0} días de desfase.",
                    paypalMatch.CounterpartyName
                );
            }
        }

        // 3. Si el movimiento es de Amazon
        if (upperDesc.Contains("AMZN") || upperDesc.Contains("AMAZON"))
        {
            var amazonOrders = eligibleOrders
                .Where(o => o.Provider == CommerceProvider.Amazon)
                .ToList();

            // 3.1 Coincidencia 1:1 directa por total de la orden
            var exactAmazonOrder = amazonOrders.FirstOrDefault(o => Math.Abs(o.TotalAmount - targetAmount) < 0.005m);
            if (exactAmazonOrder != null)
            {
                var daysDiff = Math.Abs((txDate - exactAmazonOrder.OrderDate).TotalDays);
                var confidence = daysDiff <= 2 ? MatchConfidence.Exact100 : MatchConfidence.High;
                return new ReconciliationCandidate(
                    bankTx.Id,
                    exactAmazonOrder.Id,
                    exactAmazonOrder.Items.Select(i => i.Id).ToList(),
                    confidence,
                    $"Pedido de Amazon {exactAmazonOrder.ExternalOrderId} coincide en importe total ({targetAmount:F2} €).",
                    "Amazon"
                );
            }

            // 3.2 Coincidencia 1:N por suma de subconjunto de artículos (expedición parcial)
            foreach (var order in amazonOrders)
            {
                var unreconciledItems = order.Items.Where(i => !i.IsReconciled).ToList();
                if (unreconciledItems.Count > 1)
                {
                    var subset = FindSubsetSum(unreconciledItems, targetAmount);
                    if (subset != null && subset.Count > 0)
                    {
                        return new ReconciliationCandidate(
                            bankTx.Id,
                            order.Id,
                            subset.Select(i => i.Id).ToList(),
                            MatchConfidence.Medium,
                            $"Expedición parcial de {subset.Count} productos del pedido Amazon {order.ExternalOrderId}.",
                            "Amazon"
                        );
                    }
                }
            }
        }

        // 4. Coincidencia genérica si ninguna heurística de proveedor específico cuadró pero hay orden exacta
        var genericMatch = eligibleOrders.FirstOrDefault(o => Math.Abs(o.TotalAmount - targetAmount) < 0.005m);
        if (genericMatch != null)
        {
            return new ReconciliationCandidate(
                bankTx.Id,
                genericMatch.Id,
                genericMatch.Items.Select(i => i.Id).ToList(),
                MatchConfidence.Medium,
                $"Coincidencia de importe con orden externa {genericMatch.Provider} ({genericMatch.ExternalOrderId}).",
                genericMatch.CounterpartyName ?? genericMatch.Provider.ToString()
            );
        }

        return null;
    }

    private static List<ExternalCommerceOrderItem>? FindSubsetSum(
        List<ExternalCommerceOrderItem> items,
        decimal target)
    {
        int n = items.Count;
        if (n > 12) return null; // Limite de seguridad combinatoria O(2^N)

        for (int i = 1; i < (1 << n); i++)
        {
            decimal sum = 0;
            var current = new List<ExternalCommerceOrderItem>();
            for (int j = 0; j < n; j++)
            {
                if ((i & (1 << j)) != 0)
                {
                    sum += items[j].TotalPrice;
                    current.Add(items[j]);
                }
            }
            if (Math.Abs(sum - target) < 0.005m)
            {
                return current;
            }
        }
        return null;
    }
}
