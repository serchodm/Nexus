using Nexus.Domain.Entities;
using Nexus.Domain.Enums;

namespace Nexus.Application.Statements.DTOs;

public record BankAccountDto(
    Guid Id,
    string Name,
    BankType BankType,
    string BankName,
    string? BankCode,
    string? IbanMasked,
    string Last4Digits,
    string Currency,
    AccountType Type,
    HolderRole HolderRole,
    decimal OwnershipPercentage,
    decimal? CurrentBalance,
    decimal? UserNetWorthShare, // CurrentBalance * (OwnershipPercentage / 100)
    DateTimeOffset? BalanceLastUpdatedAt,
    List<string> LinkedCardsLast4,
    DateTimeOffset CreatedAt
)
{
    public static BankAccountDto FromEntity(BankAccount account)
    {
        var netWorthShare = account.CurrentBalance.HasValue
            ? Math.Round(account.CurrentBalance.Value * (account.OwnershipPercentage / 100m), 2)
            : (decimal?)null;

        var ibanMasked = !string.IsNullOrWhiteSpace(account.Iban) && account.Iban.Length >= 8
            ? $"{account.Iban[..4]} **** **** **** {account.Last4Digits}"
            : (string.IsNullOrWhiteSpace(account.Last4Digits) ? null : $"**** {account.Last4Digits}");

        return new BankAccountDto(
            Id: account.Id,
            Name: account.Name,
            BankType: account.BankType,
            BankName: account.BankName,
            BankCode: account.BankCode,
            IbanMasked: ibanMasked,
            Last4Digits: account.Last4Digits,
            Currency: account.Currency,
            Type: account.Type,
            HolderRole: account.HolderRole,
            OwnershipPercentage: account.OwnershipPercentage,
            CurrentBalance: account.CurrentBalance,
            UserNetWorthShare: netWorthShare,
            BalanceLastUpdatedAt: account.BalanceLastUpdatedAt,
            LinkedCardsLast4: account.LinkedCardsLast4 ?? new(),
            CreatedAt: account.CreatedAt
        );
    }
}
