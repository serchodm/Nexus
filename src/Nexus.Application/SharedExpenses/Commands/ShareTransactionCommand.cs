using Nexus.Application.SharedExpenses.DTOs;
using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Nexus.Domain.Enums;
using Nexus.Domain.Repositories;
using Nexus.Domain.Services;

namespace Nexus.Application.SharedExpenses.Commands;

public record ShareTransactionCommand(
    Guid SharedSpaceId,
    Guid BankTransactionId,
    Guid? TransactionSplitItemId,
    Guid PaidByMemberId,
    decimal PayerPercentage,
    string? CustomDescription
);

public class ShareTransactionCommandHandler
{
    private readonly ISharedSpaceRepository _spaceRepository;
    private readonly ISharedExpenseRepository _expenseRepository;
    private readonly IBankTransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ShareTransactionCommandHandler(
        ISharedSpaceRepository spaceRepository,
        ISharedExpenseRepository expenseRepository,
        IBankTransactionRepository transactionRepository,
        IUnitOfWork unitOfWork)
    {
        _spaceRepository = spaceRepository;
        _expenseRepository = expenseRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SharedExpenseDto?> HandleAsync(ShareTransactionCommand command, CancellationToken ct = default)
    {
        var space = await _spaceRepository.GetByIdWithMembersAsync(command.SharedSpaceId, ct);
        if (space == null)
        {
            return null;
        }

        var member = space.Members.FirstOrDefault(m => m.Id == command.PaidByMemberId);
        if (member == null)
        {
            return null;
        }

        var tx = await _transactionRepository.GetByIdWithSplitsAsync(command.BankTransactionId, ct);
        if (tx == null)
        {
            return null;
        }

        decimal amount = Math.Abs(tx.Amount);
        string desc = string.IsNullOrWhiteSpace(command.CustomDescription)
            ? (!string.IsNullOrWhiteSpace(tx.CleanMerchantName) ? tx.CleanMerchantName : tx.Description)
            : command.CustomDescription;
        string categoryName = tx.Category?.Name ?? "General";
        string categoryIcon = tx.Category?.Icon ?? "tag";

        if (command.TransactionSplitItemId.HasValue && tx.SplitItems.Count > 0)
        {
            var splitItem = tx.SplitItems.FirstOrDefault(s => s.Id == command.TransactionSplitItemId.Value);
            if (splitItem != null)
            {
                amount = Math.Abs(splitItem.Amount);
                if (!string.IsNullOrWhiteSpace(splitItem.Description))
                {
                    desc = splitItem.Description;
                }
                if (splitItem.Category != null)
                {
                    categoryName = splitItem.Category.Name;
                    categoryIcon = splitItem.Category.Icon;
                }
            }
        }

        decimal pct = command.PayerPercentage > 0 ? command.PayerPercentage : space.DefaultOwnerPercentage;
        var (payerShare, debtorShare) = FairShareCalculator.CalculateShares(amount, pct);

        var entry = new SharedExpenseEntry
        {
            SharedSpaceId = space.Id,
            BankTransactionId = tx.Id,
            TransactionSplitItemId = command.TransactionSplitItemId,
            PaidByMemberId = member.Id,
            Description = desc,
            TransactionDate = tx.OperationDate.ToDateTime(TimeOnly.MinValue),
            TotalAmount = amount,
            PaidByShare = payerShare,
            OwedShare = debtorShare,
            CategoryName = categoryName,
            CategoryIcon = categoryIcon,
            Status = SharedExpenseStatus.Pending
        };

        await _expenseRepository.AddAsync(entry, ct);

        // Actualizar naturaleza de la transacción a Compartida
        tx.ExpenseNature = ExpenseNature.Shared;
        _transactionRepository.Update(tx);

        await _unitOfWork.SaveChangesAsync(ct);

        return new SharedExpenseDto(
            entry.Id,
            entry.SharedSpaceId,
            entry.BankTransactionId,
            entry.TransactionSplitItemId,
            entry.Description,
            entry.TransactionDate,
            entry.TotalAmount,
            entry.PaidByShare,
            entry.OwedShare,
            entry.PaidByMemberId,
            member.DisplayName,
            entry.CategoryName,
            entry.CategoryIcon,
            entry.Status.ToString().ToLowerInvariant()
        );
    }
}
