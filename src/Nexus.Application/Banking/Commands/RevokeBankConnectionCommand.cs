using Nexus.Domain.Common;
using Nexus.Domain.Enums;
using Nexus.Domain.Repositories;

namespace Nexus.Application.Banking.Commands;

public record RevokeBankConnectionCommand(
    Guid UserId,
    Guid ConnectionId
);

public class RevokeBankConnectionCommandHandler
{
    private readonly IBankConnectionRepository _connectionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RevokeBankConnectionCommandHandler(
        IBankConnectionRepository connectionRepository,
        IUnitOfWork unitOfWork)
    {
        _connectionRepository = connectionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> HandleAsync(RevokeBankConnectionCommand command, CancellationToken ct = default)
    {
        var connection = await _connectionRepository.GetByIdAsync(command.ConnectionId, ct);
        if (connection == null || connection.UserId != command.UserId)
        {
            return false;
        }

        connection.Status = BankConnectionStatus.Revoked;
        connection.EncryptedAccessToken = null;
        connection.EncryptedRefreshToken = null;

        _connectionRepository.Update(connection);
        await _unitOfWork.SaveChangesAsync(ct);

        return true;
    }
}
