using Nexus.Application.Transactions.DTOs;
using Nexus.Application.Transactions.Queries.GetTransactions;
using Nexus.Domain.Common;
using Nexus.Domain.Enums;
using Nexus.Domain.Repositories;

namespace Nexus.Application.Transactions.Commands.UpdateTransactionNature;

public record UpdateTransactionNatureCommand(
    Guid TransactionId,
    TransactionNature Nature,
    Guid UserId
);

public class UpdateTransactionNatureCommandHandler
{
    private readonly IBankTransactionRepository _transactionRepository;
    private readonly IBankAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTransactionNatureCommandHandler(
        IBankTransactionRepository transactionRepository,
        IBankAccountRepository accountRepository,
        IUnitOfWork unitOfWork)
    {
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TransactionDetailDto> HandleAsync(UpdateTransactionNatureCommand cmd, CancellationToken ct = default)
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

        tx.TransactionNature = cmd.Nature;
        tx.IsManual = true;

        _transactionRepository.Update(tx);
        await _unitOfWork.SaveChangesAsync(ct);

        var updatedTx = await _transactionRepository.GetByIdWithSplitsAsync(cmd.TransactionId, ct);
        return GetTransactionsQueryHandler.MapToDetailDto(updatedTx!);
    }
}
