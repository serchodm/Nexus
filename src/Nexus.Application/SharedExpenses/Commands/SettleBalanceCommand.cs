using Nexus.Application.SharedExpenses.DTOs;
using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Nexus.Domain.Enums;
using Nexus.Domain.Repositories;

namespace Nexus.Application.SharedExpenses.Commands;

public record SettleBalanceCommand(
    Guid SharedSpaceId,
    Guid PayerMemberId,
    Guid ReceiverMemberId,
    decimal Amount,
    SettlementMethod Method,
    string? ReferenceCode
);

public class SettleBalanceCommandHandler
{
    private readonly ISharedSpaceRepository _spaceRepository;
    private readonly ISharedExpenseRepository _expenseRepository;
    private readonly ISharedSettlementRepository _settlementRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SettleBalanceCommandHandler(
        ISharedSpaceRepository spaceRepository,
        ISharedExpenseRepository expenseRepository,
        ISharedSettlementRepository settlementRepository,
        IUnitOfWork unitOfWork)
    {
        _spaceRepository = spaceRepository;
        _expenseRepository = expenseRepository;
        _settlementRepository = settlementRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SharedSettlementDto?> HandleAsync(SettleBalanceCommand command, CancellationToken ct = default)
    {
        var space = await _spaceRepository.GetByIdWithMembersAsync(command.SharedSpaceId, ct);
        if (space == null)
        {
            return null;
        }

        var payer = space.Members.FirstOrDefault(m => m.Id == command.PayerMemberId);
        var receiver = space.Members.FirstOrDefault(m => m.Id == command.ReceiverMemberId);

        if (payer == null || receiver == null)
        {
            return null;
        }

        var settlement = new SharedSettlement
        {
            SharedSpaceId = space.Id,
            PayerMemberId = payer.Id,
            ReceiverMemberId = receiver.Id,
            Amount = Math.Abs(command.Amount),
            Method = command.Method,
            ReferenceCode = command.ReferenceCode,
            SettledAt = DateTimeOffset.UtcNow
        };

        await _settlementRepository.AddAsync(settlement, ct);

        // Amortizar partidas pendientes y vincularlas a la liquidación
        var pendingExpenses = await _expenseRepository.GetPendingBySpaceIdAsync(space.Id, ct);
        foreach (var expense in pendingExpenses)
        {
            expense.Status = SharedExpenseStatus.Settled;
            expense.SettlementId = settlement.Id;
        }
        _expenseRepository.UpdateRange(pendingExpenses);

        await _unitOfWork.SaveChangesAsync(ct);

        return new SharedSettlementDto(
            settlement.Id,
            space.Id,
            payer.Id,
            payer.DisplayName,
            receiver.Id,
            receiver.DisplayName,
            settlement.Amount,
            settlement.Method.ToString(),
            settlement.ReferenceCode,
            settlement.SettledAt
        );
    }
}
