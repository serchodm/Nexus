using System.Text.RegularExpressions;
using Nexus.Domain.Enums;

namespace Nexus.Domain.Services;

public static class BankEntityResolver
{
    private static readonly Dictionary<string, (string BankName, BankType Type)> BankRegistry = new()
    {
        ["0182"] = ("BBVA", BankType.BBVA),
        ["1465"] = ("ING Direct", BankType.ING),
        ["0049"] = ("Banco Santander", BankType.Santander),
        ["2100"] = ("CaixaBank", BankType.CaixaBank),
        ["0081"] = ("Banco Sabadell", BankType.Generic),
        ["0128"] = ("Bankinter", BankType.Generic),
        ["2080"] = ("Abanca", BankType.Generic),
        ["1491"] = ("Triodos Bank", BankType.Generic),
        ["0075"] = ("Banco Popular (Santander)", BankType.Santander),
        ["0239"] = ("EVO Banco (Bankinter)", BankType.Generic),
        ["0073"] = ("Openbank (Santander)", BankType.Santander),
    };

    /// <summary>
    /// Resuelve el nombre del banco y el BankType a partir del código de entidad (posiciones 5 a 8 en el IBAN español).
    /// </summary>
    public static (string BankName, BankType Type, string? BankCode) ResolveFromIban(string? iban)
    {
        if (string.IsNullOrWhiteSpace(iban)) return ("Banco Genérico", BankType.Generic, null);
        var clean = Regex.Replace(iban, @"\s+", "").ToUpperInvariant();
        
        // Formato ESxx EEEE...
        if (clean.StartsWith("ES") && clean.Length >= 8)
        {
            var code = clean.Substring(4, 4);
            if (BankRegistry.TryGetValue(code, out var bank))
            {
                return (bank.BankName, bank.Type, code);
            }
            return ($"Entidad {code}", BankType.Generic, code);
        }

        // Si se pasan directamente los 4 dígitos
        if (clean.Length == 4 && BankRegistry.TryGetValue(clean, out var directBank))
        {
            return (directBank.BankName, directBank.Type, clean);
        }

        return ("Banco Genérico", BankType.Generic, null);
    }
}
