namespace Nexus.Domain.Interfaces;

public record SupportedInstitution(
    string Code,
    string Name,
    string? LogoUrl,
    string Country
);

public record StartSessionResult(
    string AuthorizationUrl,
    string SessionId
);

public record AuthorizedSessionResult(
    string SessionId,
    DateTimeOffset ConsentExpiresAt,
    IReadOnlyList<DiscoveredAccount> Accounts
);

public record DiscoveredAccount(
    string ExternalAccountId,
    string? Iban,
    string AccountName,
    string Currency,
    decimal? CurrentBalance,
    decimal? AvailableBalance
);

public record RawBankTransaction(
    string? EntryReference,
    DateTime BookingDate,
    DateTime? ValueDate,
    decimal Amount,
    string Currency,
    string Description,
    string? MerchantName,
    string? CreditorIban,
    string? DebtorIban
);

public record AccountBalance(
    string ExternalAccountId,
    decimal CurrentBalance,
    decimal? AvailableBalance,
    string Currency,
    DateTimeOffset UpdatedAt
);

public interface IEnableBankingClient
{
    Task<IReadOnlyList<SupportedInstitution>> GetSupportedInstitutionsAsync(string country, CancellationToken ct = default);
    Task<StartSessionResult> StartAuthSessionAsync(string institutionCode, string redirectUri, CancellationToken ct = default);
    Task<AuthorizedSessionResult> CompleteAuthSessionAsync(string code, string sessionId, CancellationToken ct = default);
    Task<IReadOnlyList<RawBankTransaction>> GetTransactionsAsync(string accountId, DateOnly fromDate, CancellationToken ct = default);
    Task<AccountBalance> GetBalancesAsync(string accountId, CancellationToken ct = default);
}
