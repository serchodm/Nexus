namespace Nexus.Application.Auth.DTOs;

public record GoogleLoginResult(
    bool Success,
    bool MfaRequired,
    string? MfaToken,
    string? AccessToken,
    string? RawRefreshToken,
    UserProfileDto? User,
    string? ErrorMessage
);

public record RefreshTokenResult(
    bool Success,
    string? AccessToken,
    string? NewRawRefreshToken,
    UserProfileDto? User,
    string? ErrorMessage
);

public record TotpSetupResult(
    bool Success,
    string? SecretKey,
    string? QrCodeUri,
    string? ErrorMessage
);

public record TotpVerifyResult(
    bool Success,
    string? AccessToken,
    string? RawRefreshToken,
    UserProfileDto? User,
    string? ErrorMessage
);
