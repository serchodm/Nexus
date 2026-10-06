using Nexus.Application.Banking.Services;
using Nexus.Domain.Enums;
using Nexus.Domain.Interfaces;
using Xunit;

namespace Nexus.UnitTests;

public class EnableBankingTransactionMapperTests
{
    [Fact]
    public void MapToDomain_ShouldExtractMerchantAndNormalizeDates()
    {
        var raw = new RawBankTransaction(
            EntryReference: "REF-001",
            BookingDate: new DateTime(2026, 10, 5, 14, 30, 0, DateTimeKind.Utc),
            ValueDate: new DateTime(2026, 10, 5, 0, 0, 0, DateTimeKind.Utc),
            Amount: -38.50m,
            Currency: "EUR",
            Description: "TPV 12345 MERCADONA S.A. COMPRA TARJETA",
            MerchantName: null,
            CreditorIban: null,
            DebtorIban: null
        );

        var accountId = Guid.NewGuid();
        var importId = Guid.NewGuid();

        var tx = EnableBankingTransactionMapper.MapToDomain(raw, accountId, importId, balanceAfter: 1200.50m);

        Assert.Equal(accountId, tx.BankAccountId);
        Assert.Equal(importId, tx.StatementImportId);
        Assert.Equal(new DateOnly(2026, 10, 5), tx.OperationDate);
        Assert.Equal(-38.50m, tx.Amount);
        Assert.Equal("EUR", tx.Currency);
        Assert.Equal("Mercadona", tx.CleanMerchantName);
        Assert.False(string.IsNullOrWhiteSpace(tx.Fingerprint));
        Assert.Equal("REF-001", tx.Observations);
    }

    [Fact]
    public void MapToDomain_BizumIncome_ShouldClassifyAsReimbursementOrIncome()
    {
        var raw = new RawBankTransaction(
            EntryReference: "BIZ-999",
            BookingDate: new DateTime(2026, 10, 6, 20, 0, 0, DateTimeKind.Utc),
            ValueDate: null,
            Amount: 22.00m,
            Currency: "EUR",
            Description: "BIZUM DE MARCOS CENA VIERNES",
            MerchantName: null,
            CreditorIban: null,
            DebtorIban: null
        );

        var tx = EnableBankingTransactionMapper.MapToDomain(raw, Guid.NewGuid(), Guid.NewGuid());

        Assert.Equal(TransactionNature.ExpenseReimbursement, tx.TransactionNature);
    }
}
