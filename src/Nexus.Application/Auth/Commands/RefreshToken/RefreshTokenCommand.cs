using Nexus.Application.Auth.DTOs;
using Nexus.Application.Common.Interfaces;
using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Nexus.Domain.Repositories;

namespace Nexus.Application.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string RawRefreshToken, string? IpAddress);

public sealed class RefreshTokenCommandHandler
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtProvider _jwtProvider;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IJwtProvider jwtProvider,
        ITokenGenerator tokenGenerator,
        IUnitOfWork unitOfWork)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _jwtProvider = jwtProvider;
        _tokenGenerator = tokenGenerator;
        _unitOfWork = unitOfWork;
    }

    public async Task<RefreshTokenResult> HandleAsync(RefreshTokenCommand command, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(command.RawRefreshToken))
        {
            return new RefreshTokenResult(false, null, null, null, "No se proporcionó el token de refresco.");
        }

        var incomingHash = _tokenGenerator.HashToken(command.RawRefreshToken);
        var existingToken = await _refreshTokenRepository.GetByTokenHashAsync(incomingHash, ct);

        if (existingToken == null)
        {
            return new RefreshTokenResult(false, null, null, null, "Token de refresco inválido.");
        }

        // Detección de reintento de uso de token revocado (detección de robo)
        if (existingToken.IsRevoked)
        {
            return new RefreshTokenResult(false, null, null, null, "Sesión revocada o comprometida. Inicie sesión nuevamente.");
        }

        if (existingToken.IsExpired)
        {
            return new RefreshTokenResult(false, null, null, null, "La sesión ha expirado.");
        }

        var user = existingToken.User;
        if (!user.IsActive)
        {
            return new RefreshTokenResult(false, null, null, null, "Cuenta de usuario desactivada.");
        }

        // Rotación estricta de tokens
        var (newRawToken, newTokenHash) = _tokenGenerator.GenerateRefreshToken();
        var now = DateTimeOffset.UtcNow;

        existingToken.RevokedAt = now;
        existingToken.ReplacedByTokenHash = newTokenHash;

        var newRefreshToken = new Domain.Entities.RefreshToken
        {
            UserId = user.Id,
            TokenHash = newTokenHash,
            ExpiresAt = now.AddDays(14),
            CreatedByIp = command.IpAddress
        };

        await _refreshTokenRepository.AddAsync(newRefreshToken, ct);

        var newAccessToken = _jwtProvider.GenerateAccessToken(user);
        await _unitOfWork.SaveChangesAsync(ct);

        var userDto = new UserProfileDto(
            user.Id,
            user.Email,
            user.FullName,
            user.AvatarUrl,
            user.Role,
            user.IsMfaEnabled
        );

        return new RefreshTokenResult(
            Success: true,
            AccessToken: newAccessToken,
            NewRawRefreshToken: newRawToken,
            User: userDto,
            ErrorMessage: null
        );
    }
}
