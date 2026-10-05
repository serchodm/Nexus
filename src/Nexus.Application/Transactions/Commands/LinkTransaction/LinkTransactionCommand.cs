using Nexus.Application.Transactions.DTOs;
using Nexus.Application.Transactions.Queries.GetTransactions;
using Nexus.Domain.Common;
using Nexus.Domain.Enums;
using Nexus.Domain.Repositories;

namespace Nexus.Application.Transactions.Commands.LinkTransaction;

public record LinkTransactionCommand(
    Guid TransactionId,
    Guid TargetTransactionId,
    Guid UserId
);

public class LinkTransactionCommandHandler
{
    private readonly IBankTransactionRepository _transactionRepository;
    private readonly IBankAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LinkTransactionCommandHandler(
        IBankTransactionRepository transactionRepository,
        IBankAccountRepository accountRepository,
        IUnitOfWork unitOfWork)
    {
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TransactionDetailDto> HandleAsync(LinkTransactionCommand cmd, CancellationToken ct = default)
    {
        var tx = await _transactionRepository.GetByIdWithSplitsAsync(cmd.TransactionId, ct);
        if (tx == null)
        {
            throw new DomainValidationException("La transacción principal no existe.");
        }

        var account = await _accountRepository.GetByIdAsync(tx.BankAccountId, ct);
        if (account == null || account.UserId != cmd.UserId)
        {
            throw new DomainValidationException("No tienes acceso a la cuenta de la transacción principal.");
        }

        var targetTx = await _transactionRepository.GetByIdWithSplitsAsync(cmd.TargetTransactionId, ct);
        if (targetTx == null)
        {
            throw new DomainValidationException("La transacción de destino para vincular no existe.");
        }

        var targetAccount = await _accountRepository.GetByIdAsync(targetTx.BankAccountId, ct);
        if (targetAccount == null || targetAccount.UserId != cmd.UserId)
        {
            throw new DomainValidationException("No tienes acceso a la cuenta de la transacción vinculada.");
        }

        // Si es una devolución (abono vinculado a un cargo previo)
        if (tx.Amount > 0 && targetTx.Amount < 0)
        {
            if (tx.Amount > Math.Abs(targetTx.Amount))
            {
                throw new DomainValidationException($"El importe devuelto ({tx.Amount:N2} €) no puede superar el cargo original ({Math.Abs(targetTx.Amount):N2} €).");
            }

            tx.LinkedTransactionId = targetTx.Id;
            tx.TransactionNature = TransactionNature.ExpenseReimbursement;
            
            // Hereda el comercio y categoría si no los tenía
            if (targetTx.CategoryId.HasValue)
            {
                tx.CategoryId = targetTx.CategoryId;
                tx.SubcategoryId = targetTx.SubcategoryId;
                tx.CleanMerchantName = targetTx.CleanMerchantName;
                tx.CategorizationSource = CategorizationSource.Manual;
                tx.IsManual = true;
            }
        }
        // Si es un traspaso interno entre cuentas propias
        else if (Math.Abs(tx.Amount) == Math.Abs(targetTx.Amount) && ((tx.Amount < 0 && targetTx.Amount > 0) || (tx.Amount > 0 && targetTx.Amount < 0)))
        {
            tx.LinkedTransactionId = targetTx.Id;
            tx.TransactionNature = TransactionNature.InternalTransfer;

            targetTx.LinkedTransactionId = tx.Id;
            targetTx.TransactionNature = TransactionNature.InternalTransfer;
            _transactionRepository.Update(targetTx);
        }
        else
        {
            // Vinculación genérica
            tx.LinkedTransactionId = targetTx.Id;
        }

        _transactionRepository.Update(tx);
        await _unitOfWork.SaveChangesAsync(ct);

        var updatedTx = await _transactionRepository.GetByIdWithSplitsAsync(cmd.TransactionId, ct);
        return GetTransactionsQueryHandler.MapToDetailDto(updatedTx!);
    }
}
