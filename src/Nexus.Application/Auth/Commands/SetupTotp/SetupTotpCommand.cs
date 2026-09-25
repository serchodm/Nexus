using Nexus.Application.Auth.DTOs;
using Nexus.Application.Common.Interfaces;
using Nexus.Domain.Common;
using Nexus.Domain.Repositories;

namespace Nexus.Application.Auth.Commands.SetupTotp;

public record SetupTotpCommand(Guid UserId);

public sealed class SetupTotpCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly ITotpService _totpService;
    private readonly IUnitOfWork _unitOfWork;

    public SetupTotpCommandHandler(
        IUserRepository userRepository,
        ITotpService totpService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _totpService = totpService;
        _unitOfWork = unitOfWork;
    }

    public async Task<TotpSetupResult> HandleAsync(SetupTotpCommand command, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId, ct);
        if (user == null || !user.IsActive)
        {
            return new TotpSetupResult(false, null, null, "Usuario no encontrado o inactivo.");
        }

        // Generar nueva clave provisional y URI para código QR
        var secret = _totpService.GenerateSecretKey();
        var qrUri = _totpService.GenerateQrCodeUri(user.Email, secret);

        // Guardar la clave en estado provisional (is_mfa_enabled sigue en false hasta verificar el primer código)
        user.MfaSecret = secret;
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(ct);

        return new TotpSetupResult(
            Success: true,
            SecretKey: secret,
            QrCodeUri: qrUri,
            ErrorMessage: null
        );
    }
}
