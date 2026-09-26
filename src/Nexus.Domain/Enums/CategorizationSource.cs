namespace Nexus.Domain.Enums;

/// <summary>
/// Origen de la categorización asignada (para trazabilidad y prevalencia RN-01).
/// </summary>
public enum CategorizationSource
{
    None = 0,
    SystemRule = 1,      // Regla heurística global predeterminada
    BankNative = 2,      // Categoría remitida por el banco en el extracto (ej. ING)
    BizumSemantic = 3,   // Motor léxico de conceptos de Bizum
    UserRule = 4,        // Regla explícita creada por el usuario
    Manual = 5           // Asignación directa y deliberada por el usuario (Inmutable contra reglas)
}
