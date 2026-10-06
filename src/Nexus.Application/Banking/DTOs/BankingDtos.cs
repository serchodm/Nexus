namespace Nexus.Application.Banking.DTOs;

public record BankInstitutionDto(
    string Code,
    string Name,
    string? LogoUrl,
    string Country
);

public record ConnectedBankAccountDto(
    Guid Id,
    Guid BankConnectionId,
    string ExternalAccountId,
    string? Iban,
    string AccountName,
    string Currency,
    decimal? CurrentBalance,
    decimal? AvailableBalance,
    DateTimeOffset? BalanceUpdatedAt,
    Guid? InternalBankAccountId
);

public record BankConnectionDto(
    Guid Id,
    string InstitutionCode,
    string InstitutionName,
    string? LogoUrl,
    string? ExternalSessionId,
    DateTimeOffset? ConsentExpiresAt,
    DateTimeOffset? LastSyncedAt,
    string Status,
    int DaysRemaining,
    List<ConnectedBankAccountDto> Accounts
);

public record BankAuthSessionDto(
    Guid ConnectionId,
    string AuthorizationUrl,
    string SessionId
);

public record SyncResultDto(
    Guid ConnectionId,
    int TotalAccounts,
    int NewTransactionsImported,
    DateTimeOffset SyncedAt
);
