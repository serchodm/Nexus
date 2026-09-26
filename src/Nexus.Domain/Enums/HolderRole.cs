namespace Nexus.Domain.Enums;

public enum HolderRole
{
    Owner = 1,          // Titular Único (100% cómputo patrimonial)
    JointOwner = 2,     // Cotitular (computa OwnershipPercentage, por defecto 50%)
    Authorized = 3,     // Apoderado / Autorizado (0% patrimonial, gestión de gastos)
    LegalGuardian = 4   // Representante Legal de Menor (Custodia familiar)
}
