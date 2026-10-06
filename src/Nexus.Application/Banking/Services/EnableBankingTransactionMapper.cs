using System.Text.Json;
using Nexus.Domain.Entities;
using Nexus.Domain.Enums;
using Nexus.Domain.Interfaces;
using Nexus.Domain.Services;

namespace Nexus.Application.Banking.Services;

public static class EnableBankingTransactionMapper
{
    public static BankTransaction MapToDomain(
        RawBankTransaction raw,
        Guid bankAccountId,
        Guid statementImportId,
        decimal? balanceAfter = null)
    {
        var opDate = DateOnly.FromDateTime(raw.BookingDate);
        var valDate = raw.ValueDate.HasValue ? DateOnly.FromDateTime(raw.ValueDate.Value) : (DateOnly?)null;
        
        var cleanMerchant = !string.IsNullOrWhiteSpace(raw.MerchantName)
            ? raw.MerchantName
            : MerchantNormalizer.Normalize(raw.Description);

        var fingerprint = TransactionFingerprintGenerator.Generate(
            bankAccountId,
            opDate,
            raw.Amount,
            raw.Description,
            balanceAfter
        );

        var tx = new BankTransaction
        {
            BankAccountId = bankAccountId,
            StatementImportId = statementImportId,
            OperationDate = opDate,
            ValueDate = valDate,
            Description = raw.Description,
            Amount = raw.Amount,
            Currency = string.IsNullOrWhiteSpace(raw.Currency) ? "EUR" : raw.Currency,
            BalanceAfter = balanceAfter,
            Observations = raw.EntryReference,
            CleanMerchantName = cleanMerchant,
            Fingerprint = fingerprint,
            RawDataJson = JsonSerializer.Serialize(raw)
        };

        // Detección de Bizum
        if (raw.Description.Contains("BIZUM", StringComparison.OrdinalIgnoreCase))
        {
            var classification = BizumConceptClassifier.Classify(raw.Description, raw.EntryReference);
            tx.TransactionNature = raw.Amount > 0 
                ? classification.SuggestedNature 
                : TransactionNature.StandardExpense;
        }

        return tx;
    }
}
