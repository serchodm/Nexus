using Nexus.Application.Statements.DTOs;
using Nexus.Domain.Repositories;

namespace Nexus.Application.Statements.Queries.GetBankAccounts;

public class GetBankAccountsQueryHandler
{
    private readonly IBankAccountRepository _accountRepository;

    public GetBankAccountsQueryHandler(IBankAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<GetBankAccountsResult> HandleAsync(GetBankAccountsQuery query, CancellationToken ct = default)
    {
        var accounts = await _accountRepository.GetByUserIdAsync(query.UserId, ct);
        var dtos = accounts.Select(BankAccountDto.FromEntity).ToList();

        // Cómputo patrimonial ponderado según titularidad (RN-12)
        var totalNetWorth = dtos
            .Where(a => a.UserNetWorthShare.HasValue)
            .Sum(a => a.UserNetWorthShare!.Value);

        return new GetBankAccountsResult(dtos, Math.Round(totalNetWorth, 2));
    }
}
