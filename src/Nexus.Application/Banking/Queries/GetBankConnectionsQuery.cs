using Nexus.Application.Banking.DTOs;
using Nexus.Domain.Repositories;

namespace Nexus.Application.Banking.Queries;

public record GetBankConnectionsQuery(Guid UserId);

public class GetBankConnectionsQueryHandler
{
    private readonly IBankConnectionRepository _connectionRepository;

    public GetBankConnectionsQueryHandler(IBankConnectionRepository connectionRepository)
    {
        _connectionRepository = connectionRepository;
    }

    public async Task<List<BankConnectionDto>> HandleAsync(GetBankConnectionsQuery query, CancellationToken ct = default)
    {
        var connections = await _connectionRepository.GetByUserIdAsync(query.UserId, ct);

        return connections.Select(c =>
        {
            var daysRemaining = c.ConsentExpiresAt.HasValue
                ? Math.Max(0, (int)(c.ConsentExpiresAt.Value - DateTimeOffset.UtcNow).TotalDays)
                : 0;

            return new BankConnectionDto(
                c.Id,
                c.InstitutionCode,
                c.InstitutionName,
                c.LogoUrl,
                c.ExternalSessionId,
                c.ConsentExpiresAt,
                c.LastSyncedAt,
                c.Status.ToString(),
                daysRemaining,
                c.Accounts.Select(a => new ConnectedBankAccountDto(
                    a.Id,
                    a.BankConnectionId,
                    a.ExternalAccountId,
                    a.Iban,
                    a.AccountName,
                    a.Currency,
                    a.CurrentBalance,
                    a.AvailableBalance,
                    a.BalanceUpdatedAt,
                    a.InternalBankAccountId
                )).ToList()
            );
        }).ToList();
    }
}
