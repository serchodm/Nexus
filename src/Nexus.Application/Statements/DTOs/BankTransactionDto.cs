using Nexus.Domain.Entities;

namespace Nexus.Application.Statements.DTOs;

public record BankTransactionDto(
    Guid Id,
    Guid BankAccountId,
    Guid StatementImportId,
    DateOnly OperationDate,
    DateOnly? ValueDate,
    string Description,
    decimal Amount,
    string Currency,
    decimal? BalanceAfter,
    string? BankCategory,
    string? BankSubcategory,
    string? BankMovementType,
    string? Observations,
    string Fingerprint,
    DateTimeOffset CreatedAt
)
{
    public static BankTransactionDto FromEntity(BankTransaction tx)
    {
        return new BankTransactionDto(
            Id: tx.Id,
            BankAccountId: tx.BankAccountId,
            StatementImportId: tx.StatementImportId,
            OperationDate: tx.OperationDate,
            ValueDate: tx.ValueDate,
            Description: tx.Description,
            Amount: tx.Amount,
            Currency: tx.Currency,
            BalanceAfter: tx.BalanceAfter,
            BankCategory: tx.BankCategory,
            BankSubcategory: tx.BankSubcategory,
            BankMovementType: tx.BankMovementType,
            Observations: tx.Observations,
            Fingerprint: tx.Fingerprint,
            CreatedAt: tx.CreatedAt
        );
    }
}
