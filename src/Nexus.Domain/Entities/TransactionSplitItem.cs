using Nexus.Domain.Common;
using Nexus.Domain.Enums;

namespace Nexus.Domain.Entities;

/// <summary>
/// Partida individual resultante del desglose de un gasto mixto (Split Transaction).
/// </summary>
public class TransactionSplitItem : BaseEntity
{
    public Guid BankTransactionId { get; set; }
    public BankTransaction BankTransaction { get; set; } = null!;

    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public Guid? SubcategoryId { get; set; }
    public Category? Subcategory { get; set; }

    public decimal Amount { get; set; }
    public string? Description { get; set; }

    public ExpenseNature ExpenseNature { get; set; } = ExpenseNature.Individual;
}
