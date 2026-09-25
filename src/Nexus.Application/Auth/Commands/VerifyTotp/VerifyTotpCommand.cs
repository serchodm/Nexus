using Nexus.Application.Auth.DTOs;
using Nexus.Application.Common.Interfaces;
using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Nexus.Domain.Repositories;

namespace Nexus.Application.Auth.Commands.VerifyTotp;

public record VerifyTotpCommand(
    string? MfaToken,
    string Code,
    Guid? AuthenticatedUserId,
    string? IpAddress
);

public sealed class VerifyTotpCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITotpService _totpService;
    private readonly IJwtProvider _jwtProvider;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public VerifyTotpCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        ITotpService totpService,
        IJwtProvider jwtProvider,
        ITokenGenerator tokenGenerator,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _totpService = totpService;
        _jwtProvider = jwtProvider;
        _tokenGenerator = tokenGenerator;
        _unitOfWork = unitOfWork;
    }

    public async Task<TotpVerifyResult> HandleAsync(VerifyTotpCommand command, CancellationToken ct = default)
    {
        Guid targetUserId;
        bool isEnrollmentFlow = false;

        // 1. Determinar el usuario objetivo según el flujo
        if (command.AuthenticatedUserId.HasValue)
        {
            // Flujo de activación inicial (usuario ya autenticado en sesión)
            targetUserId = command.AuthenticatedUserId.Value;
            isEnrollmentFlow = true;
        }
        else if (!string.IsNullOrWhiteSpace(command.MfaToken))
        {
            // Flujo de segundo paso de login (desafío MFA tras Google OAuth)
            var validatedUserId = _jwtProvider.ValidateMfaPendingToken(command.MfaToken);
            if (!validatedUserId.HasValue)
            {
                return new TotpVerifyResult(false, null, null, null, "El token de verificación MFA ha expirado o es inválido.");
            }
            targetUserId = validatedUserId.Value;
        }
        else
        {
            return new TotpVerifyResult(false, null, null, null, "Faltan credenciales de verificación.");
        }

        var user = await _userRepository.GetByIdAsync(targetUserId, ct);
        if (user == null || !user.IsActive || string.IsNullOrWhiteSpace(user.MfaSecret))
        {
            return new TotpVerifyResult(false, null, null, null, "No hay configuración MFA activa para este usuario.");
        }

        // 2. Validar código con protección anti-repetición (RFC 6238)
        var isValid = _totpService.VerifyCode(user.MfaSecret, command.Code, user.LastTotpStep, out var matchedStep);
        if (!isValid)
        {
            return new TotpVerifyResult(false, null, null, null, "Código de 6 dígitos inválido, caducado o ya utilizado.");
        }

        // 3. Registrar el paso temporal usado para evitar reutilizaciones
        user.LastTotpStep = matchedStep;

        if (isEnrollmentFlow)
        {
            // Enrolamiento exitoso: confirmar activación definitiva
            user.IsMfaEnabled = true;
            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(ct);

            var userDto = new UserProfileDto(user.Id, user.Email, user.FullName, user.AvatarUrl, user.Role, user.IsMfaEnabled);
            return new TotpVerifyResult(true, null, null, userDto, null);
        }

        // 4. Si fue el paso 2 de login: emitir Access Token y Refresh Token definitivos
        _userRepository.Update(user);

        var accessToken = _jwtProvider.GenerateAccessToken(user);
        var (rawToken, tokenHash) = _tokenGenerator.GenerateRefreshToken();

        var refreshToken = new Nexus.Domain.Entities.RefreshToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(14),
            CreatedByIp = command.IpAddress
        };

        await _refreshTokenRepository.AddAsync(refreshToken, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var finalUserDto = new UserProfileDto(user.Id, user.Email, user.FullName, user.AvatarUrl, user.Role, user.IsMfaEnabled);

        return new TotpVerifyResult(
            Success: true,
            AccessToken: accessToken,
            RawRefreshToken: rawToken,
            User: finalUserDto,
            ErrorMessage: null
        );
    }
}
