using System.Text.RegularExpressions;
using Nexus.Application.Statements.DTOs;
using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Nexus.Domain.Enums;
using Nexus.Domain.Repositories;
using Nexus.Domain.Services;

namespace Nexus.Application.Statements.Commands.CreateBankAccount;

public class CreateBankAccountCommandHandler
{
    private readonly IBankAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBankAccountCommandHandler(
        IBankAccountRepository accountRepository,
        IUnitOfWork unitOfWork)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateBankAccountResult> HandleAsync(CreateBankAccountCommand cmd, CancellationToken ct = default)
    {
        string? cleanIban = null;
        string last4 = string.Empty;

        if (!string.IsNullOrWhiteSpace(cmd.Iban))
        {
            cleanIban = Regex.Replace(cmd.Iban, @"\s+", "").ToUpperInvariant();
            if (cleanIban.Length >= 4)
            {
                last4 = cleanIban[^4..];
            }

            // Comprobar unicidad de IBAN para este usuario
            var exists = await _accountRepository.ExistsIbanForUserAsync(cmd.UserId, cleanIban, null, ct);
            if (exists)
            {
                return new CreateBankAccountResult(false, null, "Ya tienes una cuenta registrada con este IBAN.");
            }
        }

        // Si no se proporcionó IBAN pero sí últimos dígitos en tarjetas o nombre
        if (string.IsNullOrEmpty(last4))
        {
            last4 = "0000";
        }

        // Autodetección de entidad por código de 4 dígitos (RN-02)
        var (bankName, bankType, bankCode) = BankEntityResolver.ResolveFromIban(cleanIban);

        // Ponderación patrimonial según HolderRole (RN-11)
        var ownership = cmd.HolderRole switch
        {
            HolderRole.Owner => 100m,
            HolderRole.Authorized => 0m,
            HolderRole.JointOwner => cmd.OwnershipPercentage ?? 50m,
            HolderRole.LegalGuardian => 0m,
            _ => 100m
        };

        var account = new BankAccount
        {
            UserId = cmd.UserId,
            Name = cmd.Name.Trim(),
            BankType = bankType,
            BankName = bankName,
            BankCode = bankCode,
            Iban = cleanIban,
            Last4Digits = last4,
            Currency = "EUR",
            Type = cmd.Type,
            HolderRole = cmd.HolderRole,
            OwnershipPercentage = ownership,
            CurrentBalance = null,
            BalanceLastUpdatedAt = null,
            LinkedCardsLast4 = cmd.LinkedCardsLast4 ?? new()
        };

        await _accountRepository.AddAsync(account, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new CreateBankAccountResult(true, BankAccountDto.FromEntity(account));
    }
}
