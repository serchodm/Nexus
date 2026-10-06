using Nexus.Application.SharedExpenses.DTOs;
using Nexus.Domain.Entities;
using Nexus.Domain.Repositories;
using Nexus.Domain.Services;

namespace Nexus.Application.SharedExpenses.Queries;

public record GetActiveSharedSpaceQuery(Guid UserId);

public class GetActiveSharedSpaceQueryHandler
{
    private readonly ISharedSpaceRepository _spaceRepository;
    private readonly ISharedExpenseRepository _expenseRepository;

    public GetActiveSharedSpaceQueryHandler(
        ISharedSpaceRepository spaceRepository,
        ISharedExpenseRepository expenseRepository)
    {
        _spaceRepository = spaceRepository;
        _expenseRepository = expenseRepository;
    }

    public async Task<SharedSpaceSummaryDto?> HandleAsync(GetActiveSharedSpaceQuery query, CancellationToken ct = default)
    {
        var space = await _spaceRepository.GetActiveForUserAsync(query.UserId, ct);
        if (space == null)
        {
            return null;
        }

        var activeMember = space.Members.FirstOrDefault(m => m.UserId == query.UserId) ?? space.Members.FirstOrDefault();
        var activeMemberId = activeMember?.Id ?? Guid.Empty;

        var pendingExpenses = await _expenseRepository.GetPendingBySpaceIdAsync(space.Id, ct);
        decimal netBalance = SharedLedgerBalanceCalculator.CalculateNetBalance(pendingExpenses, activeMemberId);

        var (totalShared, userPaid, partnerPaid) = SharedLedgerBalanceCalculator.CalculateTotals(pendingExpenses, activeMemberId);

        var memberDtos = space.Members.Select(m => new SharedSpaceMemberDto(
            m.Id,
            m.DisplayName,
            m.Initials,
            m.AvatarColor,
            m.Role.ToString(),
            m.DeclaredMonthlyIncome
        )).ToList();

        var pendingDtos = pendingExpenses.Select(e => new SharedExpenseDto(
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

        string splitRatioStr = $"{Math.Round(space.DefaultOwnerPercentage, 0)}/{Math.Round(space.DefaultMemberPercentage, 0)}";

        return new SharedSpaceSummaryDto(
            space.Id,
            space.Name,
            splitRatioStr,
            netBalance,
            totalShared,
            userPaid,
            partnerPaid,
            memberDtos,
            pendingDtos
        );
    }
}
