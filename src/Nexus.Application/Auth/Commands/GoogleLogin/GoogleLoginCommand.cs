using Nexus.Application.Auth.DTOs;
using Nexus.Application.Common.Interfaces;
using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Nexus.Domain.Repositories;

namespace Nexus.Application.Auth.Commands.GoogleLogin;

public record GoogleLoginCommand(string IdToken, string? IpAddress);

public sealed class GoogleLoginCommandHandler
{
    private readonly IGoogleAuthService _googleAuthService;
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtProvider _jwtProvider;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public GoogleLoginCommandHandler(
        IGoogleAuthService _googleAuthService,
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IJwtProvider jwtProvider,
        ITokenGenerator tokenGenerator,
        IUnitOfWork unitOfWork)
    {
        this._googleAuthService = _googleAuthService;
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtProvider = jwtProvider;
        _tokenGenerator = tokenGenerator;
        _unitOfWork = unitOfWork;
    }

    public async Task<GoogleLoginResult> HandleAsync(GoogleLoginCommand command, CancellationToken ct = default)
    {
        // 1. Validar el token con Google OAuth
        var payload = await _googleAuthService.ValidateTokenAsync(command.IdToken, ct);
        if (payload == null)
        {
            return new GoogleLoginResult(false, false, null, null, null, null, "El token de Google es inválido o ha expirado.");
        }

        // 2. Buscar usuario por Google Subject ID o por Email
        var user = await _userRepository.GetByGoogleSubjectIdAsync(payload.SubjectId, ct);
        if (user == null)
        {
            user = await _userRepository.GetByEmailAsync(payload.Email, ct);
        }

        // 3. Crear usuario si es nuevo o actualizar datos si existe
        if (user == null)
        {
            user = new User
            {
                Email = payload.Email,
                FullName = payload.FullName,
                AvatarUrl = payload.AvatarUrl,
                GoogleSubjectId = payload.SubjectId,
                Role = "User",
                IsActive = true,
                IsMfaEnabled = false
            };
            await _userRepository.AddAsync(user, ct);
        }
        else
        {
            if (!user.IsActive)
            {
                return new GoogleLoginResult(false, false, null, null, null, null, "La cuenta de usuario se encuentra desactivada.");
            }

            // Actualizar datos del perfil y vincular SubjectId de Google si no estaba asociado
            user.FullName = payload.FullName;
            user.AvatarUrl = payload.AvatarUrl;
            user.GoogleSubjectId ??= payload.SubjectId;
            _userRepository.Update(user);
        }

        // 4. Si el usuario tiene MFA activado, emitir token temporal de paso 2
        if (user.IsMfaEnabled)
        {
            var mfaToken = _jwtProvider.GenerateMfaPendingToken(user);
            await _unitOfWork.SaveChangesAsync(ct);

            return new GoogleLoginResult(
                Success: true,
                MfaRequired: true,
                MfaToken: mfaToken,
                AccessToken: null,
                RawRefreshToken: null,
                User: null,
                ErrorMessage: null
            );
        }

        // 5. Flujo directo sin MFA: Emitir Access Token en memoria y Refresh Token para Cookie
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

        var userDto = new UserProfileDto(
            user.Id,
            user.Email,
            user.FullName,
            user.AvatarUrl,
            user.Role,
            user.IsMfaEnabled
        );

        return new GoogleLoginResult(
            Success: true,
            MfaRequired: false,
            MfaToken: null,
            AccessToken: accessToken,
            RawRefreshToken: rawToken,
            User: userDto,
            ErrorMessage: null
        );
    }
}
