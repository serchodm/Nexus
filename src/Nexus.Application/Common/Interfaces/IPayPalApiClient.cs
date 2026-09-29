namespace Nexus.Application.Common.Interfaces;

public record PayPalOAuthTokenResponse(
    string AccessToken,
    string? RefreshToken,
    int ExpiresIn
);

public record PayPalTransactionItem(
    string TransactionId,
    DateTime TransactionDate,
    decimal GrossAmount,
    decimal FeeAmount,
    string Currency,
    string? CounterpartyName,
    string? ItemSubject,
    string Status
);

public interface IPayPalApiClient
{
    Task<PayPalOAuthTokenResponse?> ExchangeAuthorizationCodeAsync(string clientId, string clientSecret, string authCode, string redirectUri, CancellationToken ct = default);
    Task<List<PayPalTransactionItem>> GetTransactionsAsync(string accessToken, DateTime startDate, DateTime endDate, CancellationToken ct = default);
}
