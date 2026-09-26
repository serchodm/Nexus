using Nexus.Application.Rules.Commands.CreateCategorizationRule;
using Nexus.Application.Transactions.DTOs;
using Nexus.Application.Transactions.Queries.GetTransactions;
using Nexus.Domain.Common;
using Nexus.Domain.Enums;
using Nexus.Domain.Repositories;
using Nexus.Domain.Services;

namespace Nexus.Application.Transactions.Commands.CategorizeTransaction;

public record CategorizeTransactionCommand(
    Guid TransactionId,
    Guid UserId,
    Guid CategoryId,
    Guid? SubcategoryId = null,
    ExpenseNature? ExpenseNature = null,
    ExpenseFlexibility? ExpenseFlexibility = null,
    bool? IsRecurring = null,
    bool CreateRuleForMerchant = false
);

public class CategorizeTransactionCommandHandler
{
    private readonly IBankTransactionRepository _transactionRepository;
    private readonly IBankAccountRepository _accountRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateCategorizationRuleCommandHandler _createRuleHandler;

    public CategorizeTransactionCommandHandler(
        IBankTransactionRepository transactionRepository,
        IBankAccountRepository accountRepository,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        CreateCategorizationRuleCommandHandler createRuleHandler)
    {
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _createRuleHandler = createRuleHandler;
    }

    public async Task<TransactionDetailDto> HandleAsync(CategorizeTransactionCommand cmd, CancellationToken ct = default)
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

        var category = await _categoryRepository.GetByIdAsync(cmd.CategoryId, ct);
        if (category == null)
        {
            throw new DomainValidationException("La categoría especificada no existe.");
        }

        if (cmd.SubcategoryId.HasValue)
        {
            var subcategory = await _categoryRepository.GetByIdAsync(cmd.SubcategoryId.Value, ct);
            if (subcategory == null || subcategory.ParentCategoryId != cmd.CategoryId)
            {
                throw new DomainValidationException("La subcategoría no pertenece a la categoría seleccionada.");
            }
        }

        // RN-01: Marcado manual deliberado
        tx.CategoryId = cmd.CategoryId;
        tx.SubcategoryId = cmd.SubcategoryId;
        tx.CategorizationSource = CategorizationSource.Manual;
        tx.IsManual = true;

        if (cmd.ExpenseNature.HasValue)
        {
            tx.ExpenseNature = cmd.ExpenseNature.Value;
        }

        if (cmd.ExpenseFlexibility.HasValue)
        {
            tx.ExpenseFlexibility = cmd.ExpenseFlexibility.Value;
        }

        if (cmd.IsRecurring.HasValue)
        {
            tx.IsRecurring = cmd.IsRecurring.Value;
            if (tx.IsRecurring && tx.RecurrencePeriod == RecurrencePeriod.None)
            {
                tx.RecurrencePeriod = RecurrencePeriod.Monthly;
            }
        }

        if (string.IsNullOrWhiteSpace(tx.CleanMerchantName))
        {
            tx.CleanMerchantName = MerchantNormalizer.Normalize(tx.Description);
        }

        _transactionRepository.Update(tx);
        await _unitOfWork.SaveChangesAsync(ct);

        // Si el usuario solicitó crear una regla para el comercio:
        if (cmd.CreateRuleForMerchant)
        {
            var pattern = !string.IsNullOrWhiteSpace(tx.CleanMerchantName)
                ? tx.CleanMerchantName
                : tx.Description.Trim();

            await _createRuleHandler.HandleAsync(new CreateCategorizationRuleCommand(
                UserId: cmd.UserId,
                Name: $"Regla automática: {pattern}",
                Pattern: pattern,
                MatchType: RuleMatchType.Contains,
                TargetCategoryId: cmd.CategoryId,
                TargetSubcategoryId: cmd.SubcategoryId,
                Priority: 100,
                SetExpenseNature: tx.ExpenseNature,
                SetIsRecurring: tx.IsRecurring,
                SetExpenseFlexibility: tx.ExpenseFlexibility,
                ApplyRetroactive: true
            ), ct);
        }

        // Recargar con navegación completa
        var updatedTx = await _transactionRepository.GetByIdWithSplitsAsync(cmd.TransactionId, ct);
        return GetTransactionsQueryHandler.MapToDetailDto(updatedTx!);
    }
}
