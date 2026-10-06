using Nexus.Application.Banking.DTOs;
using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Nexus.Domain.Enums;
using Nexus.Domain.Interfaces;
using Nexus.Domain.Repositories;

namespace Nexus.Application.Banking.Commands;

public record CompleteBankAuthCallbackCommand(
    Guid UserId,
    string Code,
    string SessionId
);

public class CompleteBankAuthCallbackCommandHandler
{
    private readonly IEnableBankingClient _enableBankingClient;
    private readonly IBankConnectionRepository _connectionRepository;
    private readonly IConnectedBankAccountRepository _accountRepository;
    private readonly IBankAccountRepository _internalBankAccountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteBankAuthCallbackCommandHandler(
        IEnableBankingClient enableBankingClient,
        IBankConnectionRepository connectionRepository,
        IConnectedBankAccountRepository accountRepository,
        IBankAccountRepository internalBankAccountRepository,
        IUnitOfWork unitOfWork)
    {
        _enableBankingClient = enableBankingClient;
        _connectionRepository = connectionRepository;
        _accountRepository = accountRepository;
        _internalBankAccountRepository = internalBankAccountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BankConnectionDto?> HandleAsync(CompleteBankAuthCallbackCommand command, CancellationToken ct = default)
    {
        var authResult = await _enableBankingClient.CompleteAuthSessionAsync(command.Code, command.SessionId, ct);
        
        var connection = await _connectionRepository.GetBySessionIdAsync(command.SessionId, ct);
        if (connection == null)
        {
            // Si la conexión fue iniciada externamente o id desincronizado, recuperamos por usuario
            var userConnections = await _connectionRepository.GetByUserIdAsync(command.UserId, ct);
            connection = userConnections.FirstOrDefault(c => c.ExternalSessionId == command.SessionId || c.Status == BankConnectionStatus.PendingAuthorization);
        }

        if (connection == null)
        {
            connection = new BankConnection
            {
                UserId = command.UserId,
                InstitutionCode = "generic_bank",
                InstitutionName = "Entidad Bancaria Conectada",
                ExternalSessionId = command.SessionId
            };
            await _connectionRepository.AddAsync(connection, ct);
        }

        connection.Status = BankConnectionStatus.Active;
        connection.ConsentExpiresAt = authResult.ConsentExpiresAt;
        connection.LastSyncedAt = DateTimeOffset.UtcNow;

        // Sincronizar o insertar cuentas descubiertas
        var userInternalAccounts = await _internalBankAccountRepository.GetByUserIdAsync(command.UserId, ct);

        foreach (var disc in authResult.Accounts)
        {
            var existingAcc = connection.Accounts.FirstOrDefault(a => a.ExternalAccountId == disc.ExternalAccountId);
            
            // Buscar si ya existe una cuenta contable en Nexus con el mismo IBAN
            Guid? linkedInternalId = null;
            if (!string.IsNullOrWhiteSpace(disc.Iban))
            {
                var matchedInternal = userInternalAccounts.FirstOrDefault(ia => 
                    !string.IsNullOrWhiteSpace(ia.Iban) && 
                    ia.Iban.Replace(" ", "").Equals(disc.Iban.Replace(" ", ""), StringComparison.OrdinalIgnoreCase));
                
                if (matchedInternal != null)
                {
                    linkedInternalId = matchedInternal.Id;
                    if (disc.CurrentBalance.HasValue)
                    {
                        matchedInternal.CurrentBalance = disc.CurrentBalance.Value;
                        matchedInternal.BalanceLastUpdatedAt = DateTimeOffset.UtcNow;
                    }
                }
                else
                {
                    // Crear automáticamente la cuenta contable en Nexus para el usuario
                    var last4 = disc.Iban.Length >= 4 ? disc.Iban[^4..] : "0000";
                    var newInternal = new BankAccount
                    {
                        UserId = command.UserId,
                        Name = disc.AccountName,
                        BankName = connection.InstitutionName,
                        Iban = disc.Iban,
                        Last4Digits = last4,
                        Currency = disc.Currency,
                        CurrentBalance = disc.CurrentBalance,
                        BalanceLastUpdatedAt = DateTimeOffset.UtcNow,
                        Type = AccountType.Checking,
                        HolderRole = HolderRole.Owner
                    };
                    await _internalBankAccountRepository.AddAsync(newInternal, ct);
                    linkedInternalId = newInternal.Id;
                }
            }

            if (existingAcc == null)
            {
                var newConnectedAcc = new ConnectedBankAccount
                {
                    BankConnectionId = connection.Id,
                    ExternalAccountId = disc.ExternalAccountId,
                    Iban = disc.Iban,
                    AccountName = disc.AccountName,
                    Currency = disc.Currency,
                    CurrentBalance = disc.CurrentBalance,
                    AvailableBalance = disc.AvailableBalance,
                    BalanceUpdatedAt = DateTimeOffset.UtcNow,
                    InternalBankAccountId = linkedInternalId
                };
                connection.Accounts.Add(newConnectedAcc);
            }
            else
            {
                existingAcc.CurrentBalance = disc.CurrentBalance ?? existingAcc.CurrentBalance;
                existingAcc.AvailableBalance = disc.AvailableBalance ?? existingAcc.AvailableBalance;
                existingAcc.BalanceUpdatedAt = DateTimeOffset.UtcNow;
                if (linkedInternalId.HasValue) existingAcc.InternalBankAccountId = linkedInternalId;
            }
        }

        _connectionRepository.Update(connection);
        await _unitOfWork.SaveChangesAsync(ct);

        var daysRemaining = connection.ConsentExpiresAt.HasValue 
            ? Math.Max(0, (int)(connection.ConsentExpiresAt.Value - DateTimeOffset.UtcNow).TotalDays)
            : 180;

        return new BankConnectionDto(
            connection.Id,
            connection.InstitutionCode,
            connection.InstitutionName,
            connection.LogoUrl,
            connection.ExternalSessionId,
            connection.ConsentExpiresAt,
            connection.LastSyncedAt,
            connection.Status.ToString(),
            daysRemaining,
            connection.Accounts.Select(a => new ConnectedBankAccountDto(
                a.Id,
                a.BankConnectionId,
                a.ExternalAccountId,
                a.Iban,
                a.AccountName,
                a.Currency,
                a.CurrentBalance,
                a.AvailableBalance,
                a.BalanceUpdatedAt,
                a.InternalBankAccountId
            )).ToList()
        );
    }
}
