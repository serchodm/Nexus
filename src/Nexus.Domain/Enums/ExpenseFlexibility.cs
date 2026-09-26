namespace Nexus.Domain.Enums;

/// <summary>
/// Flexibilidad presupuestaria del gasto (coste basal vs discrecional).
/// </summary>
public enum ExpenseFlexibility
{
    Discretionary = 1, // Gasto variable prescindible (restaurantes, ocio, caprichos)
    Fixed = 2          // Coste de vida basal o comprometido (alquiler, luz, seguros, suscripciones)
}
