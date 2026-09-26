using Nexus.Application.Statements.DTOs;
using Nexus.Application.Statements.Queries.GetBankAccounts;
using Nexus.Domain.Entities;
using Nexus.Domain.Enums;
using Nexus.Domain.Repositories;
using Xunit;

namespace Nexus.UnitTests;

public class NetWorthCalculationTests
{
    private class MockAccountRepository : IBankAccountRepository
    {
        private readonly List<BankAccount> _accounts;

        public MockAccountRepository(List<BankAccount> accounts)
        {
            _accounts = accounts;
        }

        public Task<BankAccount?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(_accounts.FirstOrDefault(a => a.Id == id));

        public Task<List<BankAccount>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
            => Task.FromResult(_accounts.Where(a => a.UserId == userId).ToList());

        public Task<bool> ExistsIbanForUserAsync(Guid userId, string iban, Guid? excludeAccountId = null, CancellationToken ct = default)
            => Task.FromResult(_accounts.Any(a => a.UserId == userId && a.Iban == iban && a.Id != excludeAccountId));

        public Task AddAsync(BankAccount account, CancellationToken ct = default)
        {
            _accounts.Add(account);
            return Task.CompletedTask;
        }

        public void Update(BankAccount account) { }
    }

    [Fact]
    public async Task GetBankAccounts_CalculatesWeightedNetWorthAccordingToHolderRole()
    {
        var userId = Guid.NewGuid();

        // 1. Titular único: 1.000 € -> 100% = 1.000 €
        var acc1 = new BankAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Cuenta Personal",
            BankName = "BBVA",
            BankType = BankType.BBVA,
            Last4Digits = "1234",
            HolderRole = HolderRole.Owner,
            OwnershipPercentage = 100m,
            CurrentBalance = 1000m
        };

        // 2. Cotitular al 50%: 2.000 € -> 50% = 1.000 €
        var acc2 = new BankAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Gastos Comunes Pareja",
            BankName = "ING Direct",
            BankType = BankType.ING,
            Last4Digits = "5678",
            HolderRole = HolderRole.JointOwner,
            OwnershipPercentage = 50m,
            CurrentBalance = 2000m
        };

        // 3. Apoderado: 50.000 € -> 0% (no infla patrimonio)
        var acc3 = new BankAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Empresa Familiar (Apoderado)",
            BankName = "Santander",
            BankType = BankType.Santander,
            Last4Digits = "9999",
            HolderRole = HolderRole.Authorized,
            OwnershipPercentage = 0m,
            CurrentBalance = 50000m
        };

        var repo = new MockAccountRepository(new List<BankAccount> { acc1, acc2, acc3 });
        var handler = new GetBankAccountsQueryHandler(repo);

        var result = await handler.HandleAsync(new GetBankAccountsQuery(userId));

        Assert.Equal(3, result.Accounts.Count);
        // Total esperado: 1000 + (2000 * 0.5) + 0 = 2000 €
        Assert.Equal(2000m, result.TotalNetWorthShare);
    }
}
