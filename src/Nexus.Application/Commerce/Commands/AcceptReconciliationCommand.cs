using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Nexus.Domain.Enums;
using Nexus.Domain.Repositories;

namespace Nexus.Application.Commerce.Commands;

public record AcceptReconciliationResult(
    bool Success,
    Guid TransactionId,
    int CreatedSplitsCount,
    string? MerchantName,
    string? Message
);

public record AcceptReconciliationCommand(
    Guid UserId,
    Guid ReconciliationId
);

public class AcceptReconciliationCommandHandler
{
    private readonly ITransactionReconciliationRepository _reconciliationRepository;
    private readonly IBankTransactionRepository _transactionRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AcceptReconciliationCommandHandler(
        ITransactionReconciliationRepository reconciliationRepository,
        IBankTransactionRepository transactionRepository,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _reconciliationRepository = reconciliationRepository;
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AcceptReconciliationResult> HandleAsync(AcceptReconciliationCommand request, CancellationToken cancellationToken = default)
    {
        var rec = await _reconciliationRepository.GetByIdAsync(request.ReconciliationId, cancellationToken);
        if (rec == null || rec.UserId != request.UserId)
        {
            return new AcceptReconciliationResult(false, Guid.Empty, 0, null, "La reconciliación solicitada no existe o no tienes acceso.");
        }

        if (rec.Status == ReconciliationStatus.Accepted)
        {
            return new AcceptReconciliationResult(true, rec.BankTransactionId, rec.BankTransaction?.SplitItems?.Count ?? 0, rec.BankTransaction?.CleanMerchantName, "La reconciliación ya estaba aceptada previamente.");
        }

        var tx = rec.BankTransaction;
        var order = rec.ExternalCommerceOrder;
        if (tx == null || order == null)
        {
            return new AcceptReconciliationResult(false, Guid.Empty, 0, null, "No se encontraron los datos asociados a la transacción u orden.");
        }

        // 1. Obtener categorías base del sistema para asignación por defecto
        var systemCategories = await _categoryRepository.GetAllSystemCategoriesAsync(cancellationToken);
        var defaultShoppingCategory = systemCategories.FirstOrDefault(c => c.Name.Contains("Compras") || c.Name.Contains("Ocio")) 
            ?? systemCategories.First();

        var targetAmount = Math.Abs(tx.Amount);
        var matchedItems = order.Items.Where(i => rec.MatchedOrderItemIds.Contains(i.Id)).ToList();
        if (matchedItems.Count == 0)
        {
            matchedItems = order.Items.ToList();
        }

        var splitItems = new List<TransactionSplitItem>();
        decimal currentSum = 0m;

        // 2. Construir partidas para cada artículo reconciliado
        foreach (var item in matchedItems)
        {
            var split = new TransactionSplitItem
            {
                BankTransactionId = tx.Id,
                Amount = item.TotalPrice,
                Description = item.Title.Length > 250 ? item.Title.Substring(0, 247) + "..." : item.Title,
                CategoryId = defaultShoppingCategory.Id,
                ExpenseNature = ExpenseNature.Individual
            };
            splitItems.Add(split);
            currentSum += item.TotalPrice;

            item.BankTransactionId = tx.Id;
        }

        // 3. Salvaguarda Invariante Contable RN-02: Garantizar tolerancia exacta 0.00 €
        var diff = targetAmount - currentSum;
        if (Math.Abs(diff) >= 0.01m)
        {
            if (diff > 0)
            {
                // Hay un remanente (ej. gastos de envío o tasas)
                splitItems.Add(new TransactionSplitItem
                {
                    BankTransactionId = tx.Id,
                    Amount = diff,
                    Description = "Gastos de Envío / Ajuste Pedido",
                    CategoryId = defaultShoppingCategory.Id,
                    ExpenseNature = ExpenseNature.Individual
                });
            }
            else
            {
                // Descuento o cupón aplicado al total
                var last = splitItems.Last();
                last.Amount += diff; // Ajustar última partida para cuadrar exactamente
            }
        }

        // 4. Reemplazar splits en la transacción
        await _transactionRepository.ReplaceSplitsAsync(tx.Id, splitItems, cancellationToken);

        // 5. Desenmascaramiento de comercio limpio (RN-06) y protección manual (RN-01)
        var cleanMerchant = order.CounterpartyName ?? (order.Provider == CommerceProvider.Amazon ? "Amazon" : "PayPal");
        tx.CleanMerchantName = cleanMerchant;
        tx.HasSplits = splitItems.Count > 1;
        tx.IsManual = true; // Proteger contra sobreescrituras automáticas futuras
        _transactionRepository.Update(tx);

        // 6. Actualizar estado de orden externa y reconciliación
        if (order.Items.All(i => i.BankTransactionId.HasValue))
        {
            order.IsFullyReconciled = true;
        }

        rec.Status = ReconciliationStatus.Accepted;
        rec.AppliedAsSplits = true;
        rec.ReconciledAt = DateTime.UtcNow;
        _reconciliationRepository.Update(rec);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AcceptReconciliationResult(
            true,
            tx.Id,
            splitItems.Count,
            cleanMerchant,
            "Reconciliación aceptada y desglosada exitosamente en el extracto bancario."
        );
    }
}
