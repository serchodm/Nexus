using System.Globalization;
using System.Text.Json;
using Nexus.Domain.Enums;
using Nexus.Domain.Security;

namespace Nexus.Application.Statements.Parsers;

public class BbvaStatementParser : IStatementParser
{
    private static readonly CultureInfo SpanishCulture = CultureInfo.GetCultureInfo("es-ES");
    private static readonly string[] DateFormats = { "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd", "dd-MM-yyyy" };

    public BankType HandledBankType => BankType.BBVA;

    public bool CanParse(string headerLine)
    {
        if (string.IsNullOrWhiteSpace(headerLine)) return false;
        var upper = headerLine.ToUpperInvariant();
        return upper.Contains("VALOR") && (upper.Contains("CONCEPTO") || upper.Contains("MOVIMIENTO")) && (upper.Contains("DISPONIBLE") || upper.Contains("IMPORTE"));
    }

    public async Task<StatementParseResult> ParseAsync(Stream fileStream, string fileName, CancellationToken ct = default)
    {
        var rawRows = await StatementTableReader.ReadRowsAsync(fileStream, fileName, ct);
        if (rawRows.Count < 2)
        {
            throw new InvalidOperationException("El archivo no contiene suficientes registros para procesar.");
        }

        var transactions = new List<ParsedRawTransaction>();
        var detectedCards = new HashSet<string>();

        // 1. Identificar fila de cabecera y mapear columnas
        var headerIndex = 0;
        for (var i = 0; i < Math.Min(6, rawRows.Count); i++)
        {
            var line = string.Join(" ", rawRows[i]).ToUpperInvariant();
            if (line.Contains("CONCEPTO") || line.Contains("MOVIMIENTO") || (line.Contains("FECHA") && line.Contains("IMPORTE")))
            {
                headerIndex = i;
                break;
            }
        }

        var headerRow = rawRows[headerIndex];
        int colFecha = -1, colValor = -1, colConcepto = -1, colMovimiento = -1, colImporte = -1, colDisponible = -1, colObs = -1;

        for (var c = 0; c < headerRow.Length; c++)
        {
            var h = headerRow[c].ToUpperInvariant().Trim();
            if (h.Contains("F.VALOR") || h.Contains("VALOR")) colValor = c;
            else if (h.Contains("FECHA")) colFecha = c;
            else if (h.Contains("CONCEPTO")) colConcepto = c;
            else if (h.Contains("MOVIMIENTO")) colMovimiento = c;
            else if (h.Contains("IMPORTE")) colImporte = c;
            else if (h.Contains("DISPONIBLE") || h.Contains("SALDO")) colDisponible = c;
            else if (h.Contains("OBSERVAC") || h.Contains("COMENTARIO")) colObs = c;
        }

        for (var i = headerIndex + 1; i < rawRows.Count; i++)
        {
            var row = rawRows[i];
            if (row.Length < 4) continue;

            // Fechas
            DateOnly operationDate = default;
            DateOnly? valueDate = null;

            if (colFecha >= 0 && colFecha < row.Length && TryParseDate(row[colFecha], out var dFecha))
            {
                operationDate = dFecha;
            }

            if (colValor >= 0 && colValor < row.Length && TryParseDate(row[colValor], out var dValor))
            {
                valueDate = dValor;
                if (operationDate == default) operationDate = dValor;
            }

            if (operationDate == default)
            {
                if (TryParseDate(row[0], out var fallbackDate)) operationDate = fallbackDate;
                else if (row.Length > 1 && TryParseDate(row[1], out var fallbackDate2)) operationDate = fallbackDate2;
                else continue;
            }

            // Concepto y Movimiento
            var concept = (colConcepto >= 0 && colConcepto < row.Length) ? row[colConcepto] : string.Empty;
            var movementType = (colMovimiento >= 0 && colMovimiento < row.Length) ? row[colMovimiento] : string.Empty;
            var fullDescription = !string.IsNullOrWhiteSpace(concept) ? concept : movementType;

            if (string.IsNullOrWhiteSpace(fullDescription) && row.Length > 2)
            {
                fullDescription = row[2];
            }

            // Importe
            decimal amount = default;
            var amountFound = false;

            if (colImporte >= 0 && colImporte < row.Length && TryParseDecimal(row[colImporte], out var parsedImp))
            {
                amount = parsedImp;
                amountFound = true;
            }
            else
            {
                // Fallback posicional
                for (var c = 2; c < row.Length; c++)
                {
                    if (TryParseDecimal(row[c], out var maybeAmount))
                    {
                        amount = maybeAmount;
                        amountFound = true;
                        colImporte = c;
                        break;
                    }
                }
            }

            if (!amountFound) continue;

            // Saldo disponible
            decimal? balanceAfter = null;
            if (colDisponible >= 0 && colDisponible < row.Length && TryParseDecimal(row[colDisponible], out var parsedBal))
            {
                balanceAfter = parsedBal;
            }
            else
            {
                for (var c = (colImporte >= 0 ? colImporte + 1 : 4); c < row.Length; c++)
                {
                    if (TryParseDecimal(row[c], out var maybeBal))
                    {
                        balanceAfter = maybeBal;
                        break;
                    }
                }
            }

            // Observaciones
            string? observations = null;
            if (colObs >= 0 && colObs < row.Length)
            {
                observations = row[colObs];
            }
            else if (row.Length > 7)
            {
                observations = row[^1];
            }

            // Detección y sanitización de tarjetas
            var cardsInObs = TextSanitizer.ExtractCardLast4(observations);
            var cardsInDesc = TextSanitizer.ExtractCardLast4(fullDescription);
            foreach (var card in cardsInObs.Concat(cardsInDesc))
            {
                detectedCards.Add(card);
            }

            var sanitizedDescription = TextSanitizer.Sanitize(fullDescription);
            var sanitizedObservations = TextSanitizer.Sanitize(observations);

            // RawData en JSON
            var rawDataDict = new Dictionary<string, string>();
            for (var c = 0; c < row.Length; c++)
            {
                rawDataDict[$"col_{c}"] = TextSanitizer.Sanitize(row[c]);
            }
            var rawDataJson = JsonSerializer.Serialize(rawDataDict);

            var firstDetectedCard = cardsInObs.Concat(cardsInDesc).FirstOrDefault();

            transactions.Add(new ParsedRawTransaction(
                OperationDate: operationDate,
                ValueDate: valueDate ?? operationDate,
                Description: sanitizedDescription,
                Amount: amount,
                Currency: "EUR",
                BalanceAfter: balanceAfter,
                BankCategory: null,
                BankSubcategory: null,
                BankMovementType: TextSanitizer.Sanitize(movementType),
                Observations: sanitizedObservations,
                RawDataJson: rawDataJson,
                DetectedCardLast4: firstDetectedCard
            ));
        }

        if (transactions.Count == 0)
        {
            throw new InvalidOperationException("No se pudieron extraer transacciones válidas del extracto de BBVA.");
        }

        // RN-08: Ordenación cronológica ascendente estricta
        var sorted = transactions.OrderBy(t => t.OperationDate).ThenBy(t => t.ValueDate).ToList();

        var startDate = sorted.First().OperationDate;
        var endDate = sorted.Last().OperationDate;

        var firstTx = sorted.First();
        var lastTx = sorted.Last();

        var startingBalance = (firstTx.BalanceAfter.HasValue) 
            ? firstTx.BalanceAfter.Value - firstTx.Amount 
            : 0m;

        var endingBalance = lastTx.BalanceAfter ?? (startingBalance + sorted.Sum(t => t.Amount));

        return new StatementParseResult(
            BankType: BankType.BBVA,
            Transactions: sorted,
            StartingBalance: Math.Round(startingBalance, 2),
            EndingBalance: Math.Round(endingBalance, 2),
            StartDate: startDate,
            EndDate: endDate,
            DetectedCards: detectedCards.ToList()
        );
    }

    private static bool TryParseDate(string input, out DateOnly date)
    {
        date = default;
        if (string.IsNullOrWhiteSpace(input)) return false;
        var clean = input.Trim();

        foreach (var format in DateFormats)
        {
            if (DateOnly.TryParseExact(clean, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                return true;
            }
        }

        return DateOnly.TryParse(clean, SpanishCulture, DateTimeStyles.None, out date);
    }

    private static bool TryParseDecimal(string input, out decimal val)
    {
        val = default;
        if (string.IsNullOrWhiteSpace(input)) return false;
        var clean = input.Trim().Replace("€", "").Replace(" ", "");

        if (decimal.TryParse(clean, NumberStyles.Any, SpanishCulture, out val))
        {
            return true;
        }

        return decimal.TryParse(clean, NumberStyles.Any, CultureInfo.InvariantCulture, out val);
    }
}
