using Nexus.Application.Common.Interfaces;
using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Nexus.Domain.Enums;
using Nexus.Domain.Repositories;

namespace Nexus.Application.Commerce.Commands;

public record ConnectPayPalResult(
    bool Success,
    Guid ConnectionId,
    string AccountEmail,
    string? Message
);

public record ConnectPayPalCommand(
    Guid UserId,
    string AccountEmail,
    string? AccessToken,
    string? RefreshToken = null,
    int? ExpiresInSeconds = null
);

public class ConnectPayPalCommandHandler
{
    private readonly IExternalCommerceConnectionRepository _connectionRepository;
    private readonly ISecretEncryptionService _encryptionService;
    private readonly IUnitOfWork _unitOfWork;

    public ConnectPayPalCommandHandler(
        IExternalCommerceConnectionRepository connectionRepository,
        ISecretEncryptionService encryptionService,
        IUnitOfWork unitOfWork)
    {
        _connectionRepository = connectionRepository;
        _encryptionService = encryptionService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ConnectPayPalResult> HandleAsync(ConnectPayPalCommand request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.AccessToken))
        {
            return new ConnectPayPalResult(false, Guid.Empty, request.AccountEmail, "El token de acceso de PayPal es requerido.");
        }

        var connection = await _connectionRepository.GetByUserIdAndProviderAsync(request.UserId, CommerceProvider.PayPal, cancellationToken);
        var encryptedAccess = _encryptionService.Encrypt(request.AccessToken);
        var encryptedRefresh = string.IsNullOrWhiteSpace(request.RefreshToken) 
            ? null 
            : _encryptionService.Encrypt(request.RefreshToken);

        var expiresAt = request.ExpiresInSeconds.HasValue 
            ? DateTime.UtcNow.AddSeconds(request.ExpiresInSeconds.Value) 
            : (DateTime?)null;

        if (connection == null)
        {
            connection = new ExternalCommerceConnection
            {
                UserId = request.UserId,
                Provider = CommerceProvider.PayPal,
                AccountEmail = request.AccountEmail,
                EncryptedAccessToken = encryptedAccess,
                EncryptedRefreshToken = encryptedRefresh,
                TokenExpiresAt = expiresAt,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await _connectionRepository.AddAsync(connection, cancellationToken);
        }
        else
        {
            connection.AccountEmail = request.AccountEmail;
            connection.EncryptedAccessToken = encryptedAccess;
            connection.EncryptedRefreshToken = encryptedRefresh;
            connection.TokenExpiresAt = expiresAt;
            connection.IsActive = true;
            connection.UpdatedAt = DateTime.UtcNow;
            _connectionRepository.Update(connection);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ConnectPayPalResult(
            true,
            connection.Id,
            connection.AccountEmail,
            "Conexión con PayPal establecida exitosamente con tokens cifrados en reposo."
        );
    }
}
