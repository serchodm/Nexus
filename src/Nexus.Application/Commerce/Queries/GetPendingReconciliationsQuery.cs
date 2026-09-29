using Nexus.Domain.Enums;
using Nexus.Domain.Repositories;

namespace Nexus.Application.Commerce.Queries;

public record ReconciledOrderItemDto(
    Guid Id,
    string? Sku,
    string Title,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice
);

public record ReconciliationCandidateDto(
    Guid Id,
    Guid BankTransactionId,
    DateOnly TransactionDate,
    string TransactionDescription,
    decimal TransactionAmount,
    Guid ExternalCommerceOrderId,
    CommerceProvider Provider,
    string ExternalOrderId,
    DateTime OrderDate,
    decimal OrderAmount,
    string? CounterpartyName,
    string Confidence,
    string MatchReason,
    List<ReconciledOrderItemDto> MatchedItems
);

public record GetPendingReconciliationsQuery(
    Guid UserId
);

public class GetPendingReconciliationsQueryHandler
{
    private readonly ITransactionReconciliationRepository _reconciliationRepository;

    public GetPendingReconciliationsQueryHandler(ITransactionReconciliationRepository reconciliationRepository)
    {
        _reconciliationRepository = reconciliationRepository;
    }

    public async Task<List<ReconciliationCandidateDto>> HandleAsync(GetPendingReconciliationsQuery request, CancellationToken cancellationToken = default)
    {
        var pendings = await _reconciliationRepository.GetPendingByUserIdAsync(request.UserId, cancellationToken);

        var result = new List<ReconciliationCandidateDto>();
        foreach (var rec in pendings)
        {
            if (rec.BankTransaction == null || rec.ExternalCommerceOrder == null)
            {
                continue;
            }

            var matchedItems = rec.ExternalCommerceOrder.Items
                .Where(i => rec.MatchedOrderItemIds.Contains(i.Id) || rec.MatchedOrderItemIds.Count == 0)
                .Select(i => new ReconciledOrderItemDto(
                    i.Id,
                    i.Sku,
                    i.Title,
                    i.Quantity,
                    i.UnitPrice,
                    i.TotalPrice
                ))
                .ToList();

            result.Add(new ReconciliationCandidateDto(
                rec.Id,
                rec.BankTransactionId,
                rec.BankTransaction.OperationDate,
                rec.BankTransaction.Description,
                rec.BankTransaction.Amount,
                rec.ExternalCommerceOrderId,
                rec.ExternalCommerceOrder.Provider,
                rec.ExternalCommerceOrder.ExternalOrderId,
                rec.ExternalCommerceOrder.OrderDate,
                rec.ExternalCommerceOrder.TotalAmount,
                rec.ExternalCommerceOrder.CounterpartyName,
                rec.Confidence.ToString(),
                rec.MatchReason,
                matchedItems
            ));
        }

        return result;
    }
}
