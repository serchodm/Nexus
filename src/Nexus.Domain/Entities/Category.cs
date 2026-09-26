using Nexus.Domain.Common;
using Nexus.Domain.Enums;

namespace Nexus.Domain.Entities;

/// <summary>
/// Taxonomía jerárquica de categorías (del Sistema o personalizadas del Usuario).
/// </summary>
public class Category : BaseEntity
{
    /// <summary>
    /// ID del usuario propietario. Si es null, es una categoría global del sistema.
    /// </summary>
    public Guid? UserId { get; set; }
    public User? User { get; set; }

    /// <summary>
    /// Auto-referencia para jerarquía de 2 niveles (Padre -> Hijas).
    /// </summary>
    public Guid? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }
    public ICollection<Category> Subcategories { get; set; } = new List<Category>();

    public string Name { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;
    public string Icon { get; set; } = "tag";
    public string ColorHex { get; set; } = "#64748B";

    public FlowType FlowType { get; set; } = FlowType.Expense;
    public ExpenseNature DefaultExpenseNature { get; set; } = ExpenseNature.Individual;

    public bool IsSystem { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;

    public ICollection<BankTransaction> Transactions { get; set; } = new List<BankTransaction>();
    public ICollection<TransactionSplitItem> SplitItems { get; set; } = new List<TransactionSplitItem>();
}
