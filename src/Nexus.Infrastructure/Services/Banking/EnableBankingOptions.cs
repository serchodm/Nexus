namespace Nexus.Infrastructure.Services.Banking;

public sealed class EnableBankingOptions
{
    public const string SectionName = "EnableBanking";

    public string BaseUrl { get; set; } = "https://api.enablebanking.com";
    public string ApplicationId { get; set; } = string.Empty;
    public string? KeyPath { get; set; }
    public string? PrivateKeyPem { get; set; }
    public string DefaultRedirectUri { get; set; } = "http://localhost:5173/banking/callback";
    public bool UseMockMode { get; set; } = true;
}
