using System.Text.RegularExpressions;

namespace Nexus.Domain.Services;

/// <summary>
/// Clasificador semántico para conceptos de Bizum y transferencias entre particulares.
/// Extrae la intención del gasto y sugiere nombres de categoría y subcategoría.
/// </summary>
public static class BizumConceptClassifier
{
    public record BizumClassificationResult(
        string CategoryName,
        string SubcategoryName,
        bool IsMatch,
        Enums.TransactionNature SuggestedNature = Enums.TransactionNature.ExpenseReimbursement
    );

    // Diccionario de patrones léxicos
    private static readonly (string Pattern, string Category, string Subcategory)[] SemanticRules =
    [
        // Restauración y Ocio Gastronómico
        (@"\b(CENA|CENAS|COMIDA|COMIDAS|DESAYUNO|ALMUERZO|COPAS|CERVEZAS|TAPEO|PIZZAS?|HAMBURGUESAS?|RESTAURANTE|BAR|BODEGON|PINCHOS)\b", 
            "Alimentación", "Restaurantes y Bares"),

        // Deporte y Actividad Física
        (@"\b(PADEL|PÁDEL|FUTBOL|FÚTBOL|GYM|GIMNASIO|TENIS|PISTA|PARTIDO|BALONCESTO|CROSSFIT)\b", 
            "Ocio y Estilo de Vida", "Hobbies y Deporte"),

        // Transporte y Viajes
        (@"\b(GASOLINA|GASOIL|DIESEL|PEAJE|PEAJES|PARKING)\b", 
            "Transporte", "Combustible"),
        (@"\b(HOTEL|CASA\s+RURAL|AIRBNB|VUELO|VUELOS|VIAJE|VACACIONES)\b", 
            "Ocio y Estilo de Vida", "Viajes y Vacaciones"),

        // Hogar y Suministros
        (@"\b(ALQUILER|RENTA|FIANZA)\b", 
            "Hogar y Vivienda", "Alquiler e Hipoteca"),
        (@"\b(LUZ|GAS|AGUA|INTERNET|WIFI)\b", 
            "Hogar y Vivienda", "Electricidad y Gas"),

        // Eventos y Regalos
        (@"\b(REGALO|REGALOS|CUMPLE|CUMPLEAÑOS|BOTE|BODA|COMUNION)\b", 
            "Ocio y Estilo de Vida", "Hobbies y Deporte")
    ];

    /// <summary>
    /// Evalúa el concepto de un Bizum para determinar su categoría semántica y naturaleza contable.
    /// </summary>
    public static BizumClassificationResult Classify(string? concept, string? observations = null)
    {
        var text = $"{concept} {observations}".ToUpperInvariant();

        foreach (var (pattern, category, subcategory) in SemanticRules)
        {
            if (Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase))
            {
                return new BizumClassificationResult(category, subcategory, true, Enums.TransactionNature.ExpenseReimbursement);
            }
        }

        // Fallback genérico para transferencias/Bizum sin concepto identificable (ingreso ordinario)
        return new BizumClassificationResult("Finanzas e Ingresos", "Transferencias Recibidas", false, Enums.TransactionNature.TrueIncome);
    }
}
