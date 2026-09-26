using Nexus.Domain.Enums;

namespace Nexus.Application.Statements.Parsers;

public class StatementParserFactory
{
    private readonly IEnumerable<IStatementParser> _parsers;

    public StatementParserFactory(IEnumerable<IStatementParser> parsers)
    {
        _parsers = parsers;
    }

    /// <summary>
    /// Resuelve el parser adecuado inspeccionando la línea de cabecera con fallback al tipo de banco de la cuenta.
    /// </summary>
    public IStatementParser Resolve(string headerLine, BankType accountBankType)
    {
        // 1. Intentar autodetección por firma de cabecera
        if (!string.IsNullOrWhiteSpace(headerLine))
        {
            foreach (var parser in _parsers)
            {
                if (parser.CanParse(headerLine))
                {
                    return parser;
                }
            }
        }

        // 2. Fallback al banco configurado en la cuenta
        if (accountBankType != BankType.Generic)
        {
            var matched = _parsers.FirstOrDefault(p => p.HandledBankType == accountBankType);
            if (matched != null) return matched;
        }

        throw new InvalidOperationException(
            "No se reconoció el formato del extracto. Asegúrate de que las cabeceras coincidan con un banco soportado (BBVA o ING Direct).");
    }
}
