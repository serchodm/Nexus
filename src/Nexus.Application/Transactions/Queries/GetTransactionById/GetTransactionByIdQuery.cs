using Nexus.Application.Transactions.DTOs;
using Nexus.Application.Transactions.Queries.GetTransactions;
using Nexus.Domain.Common;
using Nexus.Domain.Repositories;

namespace Nexus.Application.Transactions.Queries.GetTransactionById;

public record GetTransactionByIdQuery(Guid TransactionId, Guid UserId);

public class GetTransactionByIdQueryHandler
{
    private readonly IBankTransactionRepository _transactionRepository;
    private readonly IBankAccountRepository _accountRepository;

    public GetTransactionByIdQueryHandler(
        IBankTransactionRepository transactionRepository,
        IBankAccountRepository accountRepository)
    {
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
    }

    public async Task<TransactionDetailDto?> HandleAsync(GetTransactionByIdQuery query, CancellationToken ct = default)
    {
        var tx = await _transactionRepository.GetByIdWithSplitsAsync(query.TransactionId, ct);
        if (tx == null)
        {
            return null;
        }

        var account = await _accountRepository.GetByIdAsync(tx.BankAccountId, ct);
        if (account == null || account.UserId != query.UserId)
        {
            return null;
        }

        return GetTransactionsQueryHandler.MapToDetailDto(tx);
    }
}
