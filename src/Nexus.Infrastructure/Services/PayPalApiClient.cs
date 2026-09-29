using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Nexus.Application.Common.Interfaces;

namespace Nexus.Infrastructure.Services;

public sealed class PayPalApiClient : IPayPalApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public PayPalApiClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        var useSandbox = configuration.GetValue<bool>("PayPal:UseSandbox", true);
        _baseUrl = useSandbox ? "https://api-m.sandbox.paypal.com" : "https://api-m.paypal.com";
    }

    public async Task<PayPalOAuthTokenResponse?> ExchangeAuthorizationCodeAsync(
        string clientId,
        string clientSecret,
        string authCode,
        string redirectUri,
        CancellationToken ct = default)
    {
        var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/v1/oauth2/token")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Basic", authHeader) },
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["code"] = authCode,
                ["redirect_uri"] = redirectUri
            })
        };

        var response = await _httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var accessToken = root.GetProperty("access_token").GetString() ?? string.Empty;
        var expiresIn = root.TryGetProperty("expires_in", out var expEl) ? expEl.GetInt32() : 3600;
        var refreshToken = root.TryGetProperty("refresh_token", out var refEl) ? refEl.GetString() : null;

        return new PayPalOAuthTokenResponse(accessToken, refreshToken, expiresIn);
    }

    public async Task<List<PayPalTransactionItem>> GetTransactionsAsync(
        string accessToken,
        DateTime startDate,
        DateTime endDate,
        CancellationToken ct = default)
    {
        var items = new List<PayPalTransactionItem>();

        var startIso = startDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
        var endIso = endDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);

        var url = $"{_baseUrl}/v1/reporting/transactions?start_date={startIso}&end_date={endIso}&fields=transaction_info,payer_info";
        using var request = new HttpRequestMessage(HttpMethod.Get, url)
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", accessToken) }
        };

        var response = await _httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode) return items;

        var json = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);
        if (!doc.RootElement.TryGetProperty("transaction_details", out var details) || details.ValueKind != JsonValueKind.Array)
        {
            return items;
        }

        foreach (var item in details.EnumerateArray())
        {
            if (!item.TryGetProperty("transaction_info", out var info)) continue;

            var txId = info.GetProperty("transaction_id").GetString() ?? string.Empty;
            var txDateStr = info.GetProperty("transaction_initiation_date").GetString();
            DateTime.TryParse(txDateStr, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var txDate);

            decimal gross = 0;
            string currency = "EUR";
            if (info.TryGetProperty("transaction_amount", out var amtEl))
            {
                decimal.TryParse(amtEl.GetProperty("value").GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out gross);
                currency = amtEl.GetProperty("currency_code").GetString() ?? "EUR";
            }

            decimal fee = 0;
            if (info.TryGetProperty("fee_amount", out var feeEl))
            {
                decimal.TryParse(feeEl.GetProperty("value").GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out fee);
            }

            string? counterparty = null;
            if (item.TryGetProperty("payer_info", out var payer) && payer.TryGetProperty("payer_name", out var payerName))
            {
                var given = payerName.TryGetProperty("given_name", out var g) ? g.GetString() : "";
                var surname = payerName.TryGetProperty("surname", out var s) ? s.GetString() : "";
                counterparty = $"{given} {surname}".Trim();
            }

            var subject = info.TryGetProperty("transaction_subject", out var subjEl) ? subjEl.GetString() : null;
            var status = info.TryGetProperty("transaction_status", out var stEl) ? stEl.GetString() ?? "S" : "S";

            items.Add(new PayPalTransactionItem(
                txId,
                txDate,
                gross,
                fee,
                currency,
                string.IsNullOrWhiteSpace(counterparty) ? null : counterparty,
                subject,
                status
            ));
        }

        return items;
    }
}
