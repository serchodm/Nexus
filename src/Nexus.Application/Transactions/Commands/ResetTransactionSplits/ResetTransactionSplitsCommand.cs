using Nexus.Application.Transactions.DTOs;
using Nexus.Application.Transactions.Queries.GetTransactions;
using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Nexus.Domain.Repositories;

namespace Nexus.Application.Transactions.Commands.ResetTransactionSplits;

public record ResetTransactionSplitsCommand(Guid TransactionId, Guid UserId);

public class ResetTransactionSplitsCommandHandler
{
    private readonly IBankTransactionRepository _transactionRepository;
    private readonly IBankAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ResetTransactionSplitsCommandHandler(
        IBankTransactionRepository transactionRepository,
        IBankAccountRepository accountRepository,
        IUnitOfWork unitOfWork)
    {
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TransactionDetailDto> HandleAsync(ResetTransactionSplitsCommand cmd, CancellationToken ct = default)
    {
        var tx = await _transactionRepository.GetByIdWithSplitsAsync(cmd.TransactionId, ct);
        if (tx == null)
        {
            throw new DomainValidationException("La transacción no existe.");
        }

        var account = await _accountRepository.GetByIdAsync(tx.BankAccountId, ct);
        if (account == null || account.UserId != cmd.UserId)
        {
            throw new DomainValidationException("No tienes acceso a la cuenta de esta transacción.");
        }

        await _transactionRepository.ReplaceSplitsAsync(tx.Id, Enumerable.Empty<TransactionSplitItem>(), ct);
        tx.HasSplits = false;

        await _unitOfWork.SaveChangesAsync(ct);

        var updated = await _transactionRepository.GetByIdWithSplitsAsync(cmd.TransactionId, ct);
        return GetTransactionsQueryHandler.MapToDetailDto(updated!);
    }
}
