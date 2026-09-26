using Nexus.Application.Transactions.DTOs;
using Nexus.Application.Transactions.Queries.GetTransactions;
using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Nexus.Domain.Repositories;
using Nexus.Domain.Services;

namespace Nexus.Application.Transactions.Commands.SplitTransaction;

public record SplitTransactionCommand(
    Guid TransactionId,
    Guid UserId,
    List<SplitItemInput> Items
);

public class SplitTransactionCommandHandler
{
    private readonly IBankTransactionRepository _transactionRepository;
    private readonly IBankAccountRepository _accountRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SplitTransactionCommandHandler(
        IBankTransactionRepository transactionRepository,
        IBankAccountRepository accountRepository,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TransactionDetailDto> HandleAsync(SplitTransactionCommand cmd, CancellationToken ct = default)
    {
        var tx = await _transactionRepository.GetByIdWithSplitsAsync(cmd.TransactionId, ct);
        if (tx == null)
        {
            throw new DomainValidationException("La transacción no existe.");
        }

        var account = await _accountRepository.GetByIdAsync(tx.BankAccountId, ct);
        if (account == null || account.UserId != cmd.UserId)
        {
            throw new DomainValidationException("No tienes acceso a la cuenta bancaria de esta transacción.");
        }

        if (cmd.Items == null || cmd.Items.Count < 2)
        {
            throw new DomainValidationException("Un desglose requiere al menos 2 partidas independientes.");
        }

        // 1. Validar invariante contable estricta (RN-02)
        var candidates = cmd.Items.Select(i => new TransactionSplitValidator.SplitCandidate(
            i.Amount,
            i.CategoryId,
            i.SubcategoryId,
            i.Description
        )).ToList();

        TransactionSplitValidator.Validate(tx.Amount, candidates);

        // 2. Validar que las categorías y subcategorías existan
        foreach (var item in cmd.Items)
        {
            var cat = await _categoryRepository.GetByIdAsync(item.CategoryId, ct);
            if (cat == null)
            {
                throw new DomainValidationException($"La categoría con ID {item.CategoryId} no existe.");
            }

            if (item.SubcategoryId.HasValue)
            {
                var subcat = await _categoryRepository.GetByIdAsync(item.SubcategoryId.Value, ct);
                if (subcat == null || subcat.ParentCategoryId != item.CategoryId)
                {
                    throw new DomainValidationException("Una o más subcategorías no pertenecen a la categoría seleccionada.");
                }
            }
        }

        // 3. Reemplazar partidas anteriores (si existían)
        tx.SplitItems.Clear();

        foreach (var item in cmd.Items)
        {
            tx.SplitItems.Add(new TransactionSplitItem
            {
                BankTransactionId = tx.Id,
                CategoryId = item.CategoryId,
                SubcategoryId = item.SubcategoryId,
                Amount = item.Amount,
                Description = string.IsNullOrWhiteSpace(item.Description) ? null : item.Description.Trim(),
                ExpenseNature = item.ExpenseNature
            });
        }

        tx.HasSplits = true;
        tx.IsManual = true;

        _transactionRepository.Update(tx);
        await _unitOfWork.SaveChangesAsync(ct);

        // 4. Recargar y mapear
        var updated = await _transactionRepository.GetByIdWithSplitsAsync(cmd.TransactionId, ct);
        return GetTransactionsQueryHandler.MapToDetailDto(updated!);
    }
}
