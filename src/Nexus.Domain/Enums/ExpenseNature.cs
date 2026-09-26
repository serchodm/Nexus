namespace Nexus.Domain.Enums;

/// <summary>
/// Naturaleza de imputación del gasto para el modelo dual (personal vs compartido).
/// </summary>
public enum ExpenseNature
{
    Individual = 1,  // Gasto propio y personal
    Shared = 2       // Gasto conjunto/familiar pre-marcado para reparto (Splitwise)
}
