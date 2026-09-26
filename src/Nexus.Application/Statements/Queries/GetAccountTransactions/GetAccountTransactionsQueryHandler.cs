using Nexus.Application.Statements.DTOs;
using Nexus.Domain.Repositories;

namespace Nexus.Application.Statements.Queries.GetAccountTransactions;

public class GetAccountTransactionsQueryHandler
{
    private readonly IBankAccountRepository _accountRepository;
    private readonly IBankTransactionRepository _transactionRepository;

    public GetAccountTransactionsQueryHandler(
        IBankAccountRepository accountRepository,
        IBankTransactionRepository transactionRepository)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<GetAccountTransactionsResult?> HandleAsync(GetAccountTransactionsQuery query, CancellationToken ct = default)
    {
        // 1. Validar existencia y propiedad de la cuenta
        var account = await _accountRepository.GetByIdAsync(query.AccountId, ct);
        if (account == null || account.UserId != query.UserId)
        {
            return null;
        }

        // 2. Normalizar paginación
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 200 ? 50 : query.PageSize;

        // 3. Consultar transacciones
        var (items, totalCount) = await _transactionRepository.GetByAccountIdAsync(
            query.AccountId,
            page,
            pageSize,
            query.FromDate,
            query.ToDate,
            ct);

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        var dtos = items.Select(BankTransactionDto.FromEntity).ToList();

        return new GetAccountTransactionsResult(
            Items: dtos,
            TotalCount: totalCount,
            Page: page,
            PageSize: pageSize,
            TotalPages: totalPages
        );
    }
}
