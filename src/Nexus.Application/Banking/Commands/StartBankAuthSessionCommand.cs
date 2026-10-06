using Nexus.Application.Banking.DTOs;
using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Nexus.Domain.Enums;
using Nexus.Domain.Interfaces;
using Nexus.Domain.Repositories;

namespace Nexus.Application.Banking.Commands;

public record StartBankAuthSessionCommand(
    Guid UserId,
    string InstitutionCode,
    string? RedirectUri
);

public class StartBankAuthSessionCommandHandler
{
    private readonly IEnableBankingClient _enableBankingClient;
    private readonly IBankConnectionRepository _connectionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public StartBankAuthSessionCommandHandler(
        IEnableBankingClient enableBankingClient,
        IBankConnectionRepository connectionRepository,
        IUnitOfWork unitOfWork)
    {
        _enableBankingClient = enableBankingClient;
        _connectionRepository = connectionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BankAuthSessionDto> HandleAsync(StartBankAuthSessionCommand command, CancellationToken ct = default)
    {
        var redirectUri = string.IsNullOrWhiteSpace(command.RedirectUri)
            ? "http://localhost:5173/banking/callback"
            : command.RedirectUri;

        var sessionResult = await _enableBankingClient.StartAuthSessionAsync(
            command.InstitutionCode,
            redirectUri,
            ct
        );

        var connection = new BankConnection
        {
            UserId = command.UserId,
            InstitutionCode = command.InstitutionCode,
            InstitutionName = command.InstitutionCode.ToUpperInvariant().Replace("_ES", ""),
            ExternalSessionId = sessionResult.SessionId,
            Status = BankConnectionStatus.PendingAuthorization,
            ConsentExpiresAt = DateTimeOffset.UtcNow.AddDays(180)
        };

        await _connectionRepository.AddAsync(connection, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new BankAuthSessionDto(
            connection.Id,
            sessionResult.AuthorizationUrl,
            sessionResult.SessionId
        );
    }
}
