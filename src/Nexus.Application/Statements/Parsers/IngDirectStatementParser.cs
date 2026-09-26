using System.Globalization;
using System.Text.Json;
using Nexus.Domain.Enums;
using Nexus.Domain.Security;

namespace Nexus.Application.Statements.Parsers;

public class IngDirectStatementParser : IStatementParser
{
    private static readonly CultureInfo SpanishCulture = CultureInfo.GetCultureInfo("es-ES");
    private static readonly string[] DateFormats = { "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd", "dd-MM-yyyy" };

    public BankType HandledBankType => BankType.ING;

    public bool CanParse(string headerLine)
    {
        if (string.IsNullOrWhiteSpace(headerLine)) return false;
        var upper = headerLine.ToUpperInvariant();
        return upper.Contains("VALOR") && upper.Contains("CATEGOR") && upper.Contains("IMPORTE");
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
            if (line.Contains("CATEGOR") && line.Contains("IMPORTE"))
            {
                headerIndex = i;
                break;
            }
        }

        var headerRow = rawRows[headerIndex];
        int colValor = -1, colOp = -1, colCat = -1, colSubcat = -1, colDesc = -1, colImporte = -1, colSaldo = -1, colComentario = -1;

        for (var c = 0; c < headerRow.Length; c++)
        {
            var h = headerRow[c].ToUpperInvariant().Trim();
            if (h.Contains("F. VALOR") || (h.Contains("VALOR") && !h.Contains("OPERACI"))) colValor = c;
            else if (h.Contains("OPERACI")) colOp = c;
            else if (h.Contains("SUBCATEGOR")) colSubcat = c;
            else if (h.Contains("CATEGOR")) colCat = c;
            else if (h.Contains("DESCRIPCI") || h.Contains("CONCEPTO")) colDesc = c;
            else if (h.Contains("IMPORTE")) colImporte = c;
            else if (h.Contains("SALDO")) colSaldo = c;
            else if (h.Contains("COMENTARIO") || h.Contains("OBSERVAC")) colComentario = c;
        }

        for (var i = headerIndex + 1; i < rawRows.Count; i++)
        {
            var row = rawRows[i];
            if (row.Length < 4) continue;

            // Fechas
            DateOnly operationDate = default;
            DateOnly? valueDate = null;

            if (colOp >= 0 && colOp < row.Length && TryParseDate(row[colOp], out var dOp))
            {
                operationDate = dOp;
            }

            if (colValor >= 0 && colValor < row.Length && TryParseDate(row[colValor], out var dValor))
            {
                valueDate = dValor;
                if (operationDate == default) operationDate = dValor;
            }

            if (operationDate == default)
            {
                if (TryParseDate(row[0], out var fallbackDate)) operationDate = fallbackDate;
                else continue;
            }

            // Categoría y Subcategoría (nativas de ING)
            string? category = (colCat >= 0 && colCat < row.Length) ? row[colCat]?.Trim() : null;
            string? subcategory = (colSubcat >= 0 && colSubcat < row.Length) ? row[colSubcat]?.Trim() : null;

            // Descripción
            string description = (colDesc >= 0 && colDesc < row.Length) ? row[colDesc] : string.Empty;
            if (string.IsNullOrWhiteSpace(description))
            {
                description = subcategory ?? category ?? "Movimiento";
            }

            // Comentario
            string? comment = (colComentario >= 0 && colComentario < row.Length) ? row[colComentario]?.Trim() : null;

            // Importe
            decimal amount = default;
            var amountFound = false;

            if (colImporte >= 0 && colImporte < row.Length && TryParseDecimal(row[colImporte], out var parsedAmount))
            {
                amount = parsedAmount;
                amountFound = true;
            }
            else
            {
                // Fallback: buscar primera columna decimal
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

            // Saldo posterior
            decimal? balanceAfter = null;
            if (colSaldo >= 0 && colSaldo < row.Length && TryParseDecimal(row[colSaldo], out var parsedSaldo))
            {
                balanceAfter = parsedSaldo;
            }
            else
            {
                for (var c = (colImporte >= 0 ? colImporte + 1 : 4); c < row.Length; c++)
                {
                    if (TryParseDecimal(row[c], out var maybeSaldo))
                    {
                        balanceAfter = maybeSaldo;
                        break;
                    }
                }
            }

            // Detección y sanitización de tarjetas
            var cardsInDesc = TextSanitizer.ExtractCardLast4(description);
            var cardsInComment = TextSanitizer.ExtractCardLast4(comment);
            foreach (var card in cardsInDesc.Concat(cardsInComment))
            {
                detectedCards.Add(card);
            }

            var sanitizedDescription = TextSanitizer.Sanitize(description);
            var sanitizedComment = TextSanitizer.Sanitize(comment);

            // RawData en JSON
            var rawDataDict = new Dictionary<string, string>();
            for (var c = 0; c < row.Length; c++)
            {
                rawDataDict[$"col_{c}"] = TextSanitizer.Sanitize(row[c]);
            }
            var rawDataJson = JsonSerializer.Serialize(rawDataDict);

            var firstDetectedCard = cardsInDesc.Concat(cardsInComment).FirstOrDefault();

            transactions.Add(new ParsedRawTransaction(
                OperationDate: operationDate,
                ValueDate: valueDate ?? operationDate,
                Description: sanitizedDescription,
                Amount: amount,
                Currency: "EUR",
                BalanceAfter: balanceAfter,
                BankCategory: category,
                BankSubcategory: subcategory,
                BankMovementType: subcategory ?? category,
                Observations: sanitizedComment,
                RawDataJson: rawDataJson,
                DetectedCardLast4: firstDetectedCard
            ));
        }

        if (transactions.Count == 0)
        {
            throw new InvalidOperationException("No se pudieron extraer transacciones válidas del extracto de ING Direct.");
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
            BankType: BankType.ING,
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
