using Nexus.Application.SharedExpenses.DTOs;
using Nexus.Domain.Repositories;

namespace Nexus.Application.SharedExpenses.Queries;

public record GetPendingExpensesQuery(Guid SharedSpaceId);

public class GetPendingExpensesQueryHandler
{
    private readonly ISharedExpenseRepository _expenseRepository;

    public GetPendingExpensesQueryHandler(ISharedExpenseRepository expenseRepository)
    {
        _expenseRepository = expenseRepository;
    }

    public async Task<List<SharedExpenseDto>> HandleAsync(GetPendingExpensesQuery query, CancellationToken ct = default)
    {
        var pendingExpenses = await _expenseRepository.GetPendingBySpaceIdAsync(query.SharedSpaceId, ct);

        return pendingExpenses.Select(e => new SharedExpenseDto(
            e.Id,
            e.SharedSpaceId,
            e.BankTransactionId,
            e.TransactionSplitItemId,
            e.Description,
            e.TransactionDate,
            e.TotalAmount,
            e.PaidByShare,
            e.OwedShare,
            e.PaidByMemberId,
            e.PaidByMember?.DisplayName ?? "Desconocido",
            e.CategoryName,
            e.CategoryIcon,
            e.Status.ToString().ToLowerInvariant()
        )).ToList();
    }
}
