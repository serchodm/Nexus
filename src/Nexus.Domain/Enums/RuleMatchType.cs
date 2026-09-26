namespace Nexus.Domain.Enums;

/// <summary>
/// Tipo de concordancia para reglas de categorización.
/// </summary>
public enum RuleMatchType
{
    Contains = 1,
    StartsWith = 2,
    Exact = 3,
    Regex = 4
}
