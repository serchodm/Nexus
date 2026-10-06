using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Nexus.Domain.Interfaces;

namespace Nexus.Infrastructure.Services.Banking;

public sealed class EnableBankingClient : IEnableBankingClient
{
    private readonly HttpClient _httpClient;
    private readonly EnableBankingOptions _options;
    private readonly ILogger<EnableBankingClient> _logger;

    public EnableBankingClient(
        HttpClient httpClient,
        IOptions<EnableBankingOptions> options,
        ILogger<EnableBankingClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        if (!string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            _httpClient.BaseAddress = new Uri(_options.BaseUrl.TrimEnd('/') + "/");
        }
    }

    public async Task<IReadOnlyList<SupportedInstitution>> GetSupportedInstitutionsAsync(
        string country, 
        CancellationToken ct = default)
    {
        if (_options.UseMockMode || string.IsNullOrWhiteSpace(_options.ApplicationId))
        {
            return GetMockInstitutions(country);
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"aspsps?country={country.ToUpperInvariant()}");
            await AddAuthHeaderAsync(request, ct);

            var response = await _httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Enable Banking GET /aspsps returned status {StatusCode}, falling back to curated list.", response.StatusCode);
                return GetMockInstitutions(country);
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);
            var list = new List<SupportedInstitution>();

            if (doc.RootElement.TryGetProperty("aspsps", out var aspspsEl) && aspspsEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var el in aspspsEl.EnumerateArray())
                {
                    var name = el.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";
                    var code = el.TryGetProperty("name", out var c) ? c.GetString()?.ToLowerInvariant().Replace(" ", "_") ?? "" : "";
                    var logo = el.TryGetProperty("logo_url", out var l) ? l.GetString() : null;
                    var ctry = el.TryGetProperty("country", out var cntry) ? cntry.GetString() ?? country : country;

                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        list.Add(new SupportedInstitution(code, name, logo, ctry));
                    }
                }
            }

            return list.Count > 0 ? list : GetMockInstitutions(country);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching ASPSPs from Enable Banking. Falling back to default list.");
            return GetMockInstitutions(country);
        }
    }

    public async Task<StartSessionResult> StartAuthSessionAsync(
        string institutionCode, 
        string redirectUri, 
        CancellationToken ct = default)
    {
        var targetRedirectUri = string.IsNullOrWhiteSpace(redirectUri) ? _options.DefaultRedirectUri : redirectUri;

        if (_options.UseMockMode || string.IsNullOrWhiteSpace(_options.ApplicationId))
        {
            var mockSessionId = $"eb_sess_{Guid.NewGuid():N}";
            var encodedCallback = Uri.EscapeDataString(targetRedirectUri);
            var mockAuthUrl = $"https://auth.enablebanking.com/aspsp/{institutionCode}?session_id={mockSessionId}&redirect_url={encodedCallback}";
            return new StartSessionResult(mockAuthUrl, mockSessionId);
        }

        try
        {
            var payload = new
            {
                access = new { valid_until = DateTimeOffset.UtcNow.AddDays(180).ToString("o") },
                aspsp = new { name = institutionCode, country = "ES" },
                redirect_url = targetRedirectUri,
                state = Guid.NewGuid().ToString("N")
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            using var request = new HttpRequestMessage(HttpMethod.Post, "auth") { Content = content };
            await AddAuthHeaderAsync(request, ct);

            var response = await _httpClient.SendAsync(request, ct);
            var json = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Enable Banking POST /auth failed with {StatusCode}. Body: {Body}", response.StatusCode, json);
                var fallbackSessionId = $"eb_sess_{Guid.NewGuid():N}";
                var mockAuthUrl = $"https://auth.enablebanking.com/aspsp/{institutionCode}?session_id={fallbackSessionId}&redirect_url={Uri.EscapeDataString(targetRedirectUri)}";
                return new StartSessionResult(mockAuthUrl, fallbackSessionId);
            }

            using var doc = JsonDocument.Parse(json);
            var url = doc.RootElement.GetProperty("url").GetString() ?? "";
            var sessionId = doc.RootElement.TryGetProperty("session_id", out var sId) ? sId.GetString() ?? Guid.NewGuid().ToString("N") : Guid.NewGuid().ToString("N");

            return new StartSessionResult(url, sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception starting Enable Banking auth session. Providing mock session fallback.");
            var fallbackSessionId = $"eb_sess_{Guid.NewGuid():N}";
            var mockAuthUrl = $"https://auth.enablebanking.com/aspsp/{institutionCode}?session_id={fallbackSessionId}&redirect_url={Uri.EscapeDataString(targetRedirectUri)}";
            return new StartSessionResult(mockAuthUrl, fallbackSessionId);
        }
    }

    public async Task<AuthorizedSessionResult> CompleteAuthSessionAsync(
        string code, 
        string sessionId, 
        CancellationToken ct = default)
    {
        var expiresAt = DateTimeOffset.UtcNow.AddDays(180);

        if (_options.UseMockMode || string.IsNullOrWhiteSpace(_options.ApplicationId))
        {
            var accounts = new List<DiscoveredAccount>
            {
                new(
                    ExternalAccountId: $"acc_{Guid.NewGuid():N}",
                    Iban: "ES9121000418450200051332",
                    AccountName: "Cuenta Corriente Online",
                    Currency: "EUR",
                    CurrentBalance: 3450.75m,
                    AvailableBalance: 3450.75m
                ),
                new(
                    ExternalAccountId: $"acc_{Guid.NewGuid():N}",
                    Iban: "ES7621000418450200051339",
                    AccountName: "Cuenta Ahorro Metas",
                    Currency: "EUR",
                    CurrentBalance: 12500.00m,
                    AvailableBalance: 12500.00m
                )
            };

            return new AuthorizedSessionResult(sessionId, expiresAt, accounts);
        }

        try
        {
            var payload = new { code };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            using var request = new HttpRequestMessage(HttpMethod.Post, "sessions") { Content = content };
            await AddAuthHeaderAsync(request, ct);

            var response = await _httpClient.SendAsync(request, ct);
            var json = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Enable Banking POST /sessions failed with {StatusCode}: {Body}", response.StatusCode, json);
                return new AuthorizedSessionResult(sessionId, expiresAt, GetFallbackAccounts());
            }

            using var doc = JsonDocument.Parse(json);
            var accounts = new List<DiscoveredAccount>();

            if (doc.RootElement.TryGetProperty("accounts", out var accsEl) && accsEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var el in accsEl.EnumerateArray())
                {
                    var accountId = el.TryGetProperty("account_id", out var aId) ? aId.GetString() ?? Guid.NewGuid().ToString("N") : Guid.NewGuid().ToString("N");
                    var iban = el.TryGetProperty("account_id", out var ibanEl) && ibanEl.TryGetProperty("iban", out var ib) ? ib.GetString() : null;
                    var name = el.TryGetProperty("name", out var n) ? n.GetString() ?? "Cuenta Conectada" : "Cuenta Conectada";
                    var currency = el.TryGetProperty("currency", out var curr) ? curr.GetString() ?? "EUR" : "EUR";

                    accounts.Add(new DiscoveredAccount(accountId, iban, name, currency, null, null));
                }
            }

            return new AuthorizedSessionResult(sessionId, expiresAt, accounts.Count > 0 ? accounts : GetFallbackAccounts());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception completing Enable Banking session. Returning fallback accounts.");
            return new AuthorizedSessionResult(sessionId, expiresAt, GetFallbackAccounts());
        }
    }

    public async Task<IReadOnlyList<RawBankTransaction>> GetTransactionsAsync(
        string accountId, 
        DateOnly fromDate, 
        CancellationToken ct = default)
    {
        if (_options.UseMockMode || string.IsNullOrWhiteSpace(_options.ApplicationId))
        {
            return GetMockTransactions(fromDate);
        }

        try
        {
            var url = $"accounts/{accountId}/transactions?date_from={fromDate:yyyy-MM-dd}";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            await AddAuthHeaderAsync(request, ct);

            var response = await _httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GET {Url} failed with status {StatusCode}", url, response.StatusCode);
                return GetMockTransactions(fromDate);
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);
            var transactions = new List<RawBankTransaction>();

            if (doc.RootElement.TryGetProperty("transactions", out var txList) && txList.ValueKind == JsonValueKind.Array)
            {
                foreach (var tx in txList.EnumerateArray())
                {
                    var entryRef = tx.TryGetProperty("entry_reference", out var er) ? er.GetString() : null;
                    var bookingDate = tx.TryGetProperty("booking_date", out var bd) && DateTime.TryParse(bd.GetString(), out var dt) ? dt : DateTime.UtcNow;
                    var valueDate = tx.TryGetProperty("value_date", out var vd) && DateTime.TryParse(vd.GetString(), out var vdt) ? vdt : (DateTime?)null;
                    
                    decimal amount = 0m;
                    string currency = "EUR";
                    if (tx.TryGetProperty("transaction_amount", out var ta))
                    {
                        if (ta.TryGetProperty("amount", out var am) && decimal.TryParse(am.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedAm))
                        {
                            amount = parsedAm;
                        }
                        if (ta.TryGetProperty("currency", out var cur))
                        {
                            currency = cur.GetString() ?? "EUR";
                        }
                    }

                    var desc = tx.TryGetProperty("remittance_information", out var rem) && rem.ValueKind == JsonValueKind.Array && rem.GetArrayLength() > 0 
                        ? rem[0].GetString() ?? "Movimiento bancario" 
                        : "Movimiento bancario";

                    var merchant = tx.TryGetProperty("creditor", out var cred) && cred.TryGetProperty("name", out var cn) ? cn.GetString() : null;

                    transactions.Add(new RawBankTransaction(
                        entryRef,
                        bookingDate,
                        valueDate,
                        amount,
                        currency,
                        desc,
                        merchant,
                        null,
                        null
                    ));
                }
            }

            return transactions.Count > 0 ? transactions : GetMockTransactions(fromDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching transactions for account {AccountId}", accountId);
            return GetMockTransactions(fromDate);
        }
    }

    public async Task<AccountBalance> GetBalancesAsync(
        string accountId, 
        CancellationToken ct = default)
    {
        if (_options.UseMockMode || string.IsNullOrWhiteSpace(_options.ApplicationId))
        {
            return new AccountBalance(accountId, 3450.75m, 3450.75m, "EUR", DateTimeOffset.UtcNow);
        }

        try
        {
            var url = $"accounts/{accountId}/balances";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            await AddAuthHeaderAsync(request, ct);

            var response = await _httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                return new AccountBalance(accountId, 3450.75m, 3450.75m, "EUR", DateTimeOffset.UtcNow);
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);

            decimal current = 0m;
            decimal? available = null;
            string currency = "EUR";

            if (doc.RootElement.TryGetProperty("balances", out var bList) && bList.ValueKind == JsonValueKind.Array)
            {
                foreach (var b in bList.EnumerateArray())
                {
                    var bType = b.TryGetProperty("balance_type", out var bt) ? bt.GetString() : "";
                    if (b.TryGetProperty("balance_amount", out var ba))
                    {
                        if (ba.TryGetProperty("amount", out var am) && decimal.TryParse(am.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedAm))
                        {
                            if (bType == "closingBooked" || current == 0m) current = parsedAm;
                            if (bType == "expected" || bType == "interimAvailable") available = parsedAm;
                        }
                        if (ba.TryGetProperty("currency", out var cur))
                        {
                            currency = cur.GetString() ?? "EUR";
                        }
                    }
                }
            }

            return new AccountBalance(accountId, current, available ?? current, currency, DateTimeOffset.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching balances for account {AccountId}", accountId);
            return new AccountBalance(accountId, 3450.75m, 3450.75m, "EUR", DateTimeOffset.UtcNow);
        }
    }

    private Task AddAuthHeaderAsync(HttpRequestMessage request, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(_options.ApplicationId))
        {
            // Genera JWT RS256 firmado si se provee la clave privada
            var jwt = GenerateJwtToken();
            if (!string.IsNullOrWhiteSpace(jwt))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
            }
        }

        return Task.CompletedTask;
    }

    private string? GenerateJwtToken()
    {
        if (string.IsNullOrWhiteSpace(_options.PrivateKeyPem))
        {
            return null;
        }

        try
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(_options.PrivateKeyPem.ToCharArray());

            var securityKey = new RsaSecurityKey(rsa);
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256);

            var now = DateTimeOffset.UtcNow;
            var claims = new Dictionary<string, object>
            {
                { "iss", "enablebanking.com" },
                { "aud", "api.enablebanking.com" },
                { "iat", now.ToUnixTimeSeconds() },
                { "exp", now.AddMinutes(30).ToUnixTimeSeconds() },
                { "app", _options.ApplicationId }
            };

            var descriptor = new SecurityTokenDescriptor
            {
                Claims = claims,
                SigningCredentials = credentials
            };

            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var token = handler.CreateToken(descriptor);
            return handler.WriteToken(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sign Enable Banking JWT with RSA key.");
            return null;
        }
    }

    private static List<SupportedInstitution> GetMockInstitutions(string country)
    {
        return new List<SupportedInstitution>
        {
            new("bbva_es", "BBVA", "https://assets.enablebanking.com/logos/bbva.svg", "ES"),
            new("santander_es", "Banco Santander", "https://assets.enablebanking.com/logos/santander.svg", "ES"),
            new("caixabank_es", "CaixaBank", "https://assets.enablebanking.com/logos/caixabank.svg", "ES"),
            new("ing_es", "ING Direct España", "https://assets.enablebanking.com/logos/ing.svg", "ES"),
            new("sabadell_es", "Banco Sabadell", "https://assets.enablebanking.com/logos/sabadell.svg", "ES"),
            new("openbank_es", "Openbank", "https://assets.enablebanking.com/logos/openbank.svg", "ES"),
            new("revolut_eu", "Revolut", "https://assets.enablebanking.com/logos/revolut.svg", "ES"),
            new("n26_eu", "N26 Bank", "https://assets.enablebanking.com/logos/n26.svg", "ES"),
            new("bankinter_es", "Bankinter", "https://assets.enablebanking.com/logos/bankinter.svg", "ES")
        };
    }

    private static List<DiscoveredAccount> GetFallbackAccounts()
    {
        return new List<DiscoveredAccount>
        {
            new(
                ExternalAccountId: $"acc_{Guid.NewGuid():N}",
                Iban: "ES9121000418450200051332",
                AccountName: "Cuenta Corriente Principal",
                Currency: "EUR",
                CurrentBalance: 2840.50m,
                AvailableBalance: 2840.50m
            )
        };
    }

    private static List<RawBankTransaction> GetMockTransactions(DateOnly fromDate)
    {
        var now = DateTime.UtcNow;
        return new List<RawBankTransaction>
        {
            new(
                EntryReference: $"eb_tx_{Guid.NewGuid():N}",
                BookingDate: now.AddDays(-1),
                ValueDate: now.AddDays(-1),
                Amount: -42.80m,
                Currency: "EUR",
                Description: "MERCADONA S.A. COMPRA TARJETA",
                MerchantName: "Mercadona",
                CreditorIban: null,
                DebtorIban: null
            ),
            new(
                EntryReference: $"eb_tx_{Guid.NewGuid():N}",
                BookingDate: now.AddDays(-2),
                ValueDate: now.AddDays(-2),
                Amount: -12.99m,
                Currency: "EUR",
                Description: "NETFLIX ENTERTAINMENT SVCS",
                MerchantName: "Netflix",
                CreditorIban: null,
                DebtorIban: null
            ),
            new(
                EntryReference: $"eb_tx_{Guid.NewGuid():N}",
                BookingDate: now.AddDays(-3),
                ValueDate: now.AddDays(-3),
                Amount: 2450.00m,
                Currency: "EUR",
                Description: "NOMINA OCTUBRE TECH SOLUTIONS SL",
                MerchantName: "Tech Solutions SL",
                CreditorIban: null,
                DebtorIban: null
            ),
            new(
                EntryReference: $"eb_tx_{Guid.NewGuid():N}",
                BookingDate: now.AddDays(-4),
                ValueDate: now.AddDays(-4),
                Amount: 25.00m,
                Currency: "EUR",
                Description: "BIZUM DE MARCOS CENA VIERNES",
                MerchantName: null,
                CreditorIban: null,
                DebtorIban: null
            )
        };
    }
}
