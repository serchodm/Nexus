using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Nexus.Domain.Repositories;
using Nexus.Domain.Services;

namespace Nexus.Application.Rules.Commands.ApplyRulesRetroactive;

public record ApplyRulesRetroactiveCommand(Guid UserId);

public record ApplyRulesRetroactiveResult(int TotalEvaluated, int TotalUpdated);

public class ApplyRulesRetroactiveCommandHandler
{
    private readonly IBankTransactionRepository _transactionRepository;
    private readonly ICategorizationRuleRepository _ruleRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ApplyRulesRetroactiveCommandHandler(
        IBankTransactionRepository transactionRepository,
        ICategorizationRuleRepository ruleRepository,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _transactionRepository = transactionRepository;
        _ruleRepository = ruleRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApplyRulesRetroactiveResult> HandleAsync(ApplyRulesRetroactiveCommand cmd, CancellationToken ct = default)
    {
        var rules = await _ruleRepository.GetActiveRulesByUserIdAsync(cmd.UserId, ct);
        var transactions = await _transactionRepository.GetAutoCategorizedOrUncategorizedByUserIdAsync(cmd.UserId, ct);
        var lookup = await _categoryRepository.GetCategoryLookupAsync(cmd.UserId, ct);

        var updated = 0;

        foreach (var tx in transactions)
        {
            // RN-01: Si es manual, está blindada
            if (tx.IsManual) continue;

            var cleanMerchant = tx.CleanMerchantName ?? MerchantNormalizer.Normalize(tx.Description);
            var eval = CategorizationEngine.Evaluate(tx, cleanMerchant, rules, lookup);

            if (eval.CategoryId.HasValue && eval.CategoryId != tx.CategoryId)
            {
                tx.CleanMerchantName = cleanMerchant;
                tx.CategoryId = eval.CategoryId;
                tx.SubcategoryId = eval.SubcategoryId;
                tx.CategorizationSource = eval.Source;
                tx.IsRecurring = eval.IsRecurring;
                tx.RecurrencePeriod = eval.RecurrencePeriod;
                tx.ExpenseFlexibility = eval.ExpenseFlexibility;
                tx.ExpenseNature = eval.ExpenseNature;

                _transactionRepository.Update(tx);
                updated++;
            }
        }

        if (updated > 0)
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }

        return new ApplyRulesRetroactiveResult(transactions.Count, updated);
    }
}
