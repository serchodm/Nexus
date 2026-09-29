using System.Globalization;
using System.Text;
using Nexus.Domain.Entities;
using Nexus.Domain.Enums;

namespace Nexus.Application.Commerce.Parsers;

public class PayPalStatementCsvParser
{
    private static readonly CultureInfo SpanishCulture = CultureInfo.GetCultureInfo("es-ES");
    private static readonly CultureInfo UsCulture = CultureInfo.InvariantCulture;

    public async Task<List<ExternalCommerceOrder>> ParseAsync(
        Stream csvStream, 
        Guid userId, 
        CancellationToken ct = default)
    {
        using var reader = new StreamReader(csvStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        var orders = new List<ExternalCommerceOrder>();

        string? headerLine = await reader.ReadLineAsync(ct);
        if (string.IsNullOrWhiteSpace(headerLine)) return orders;

        var separator = DetectSeparator(headerLine);
        var headers = ParseCsvLine(headerLine, separator);

        var colDate = FindColumnIndex(headers, "FECHA", "DATE");
        var colTime = FindColumnIndex(headers, "HORA", "TIME");
        var colName = FindColumnIndex(headers, "NOMBRE", "NAME", "REMITENTE/DESTINATARIO");
        var colType = FindColumnIndex(headers, "TIPO", "TYPE");
        var colStatus = FindColumnIndex(headers, "ESTADO", "STATUS");
        var colGross = FindColumnIndex(headers, "BRUTO", "GROSS", "MONTO");
        var colFee = FindColumnIndex(headers, "COMISIÓN", "COMISION", "FEE");
        var colCurrency = FindColumnIndex(headers, "DIVISA", "CURRENCY");
        var colTxId = FindColumnIndex(headers, "NÚMERO DE TRANSACCIÓN", "NUMERO DE TRANSACCION", "Nº DE TRANSACCIÓN", "Nº DE TRANSACCION", "ID. DE TRANSACCIÓN", "ID. DE TRANSACCION", "TRANSACTION ID", "ID DE TRANSACCIÓN");
        var colSubject = FindColumnIndex(headers, "TÍTULO DEL ARTÍCULO", "TITULO DEL ARTICULO", "ITEM TITLE", "CONCEPTO");

        if (colDate < 0 || colGross < 0)
        {
            throw new InvalidOperationException("El formato del extracto de PayPal no es válido (faltan columnas Fecha o Bruto).");
        }

        string? line;
        while ((line = await reader.ReadLineAsync(ct)) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var cols = ParseCsvLine(line, separator);
            if (cols.Count <= Math.Max(colDate, colGross)) continue;

            var rawAmount = ParseSignedAmount(cols[colGross]);
            // Solo nos interesan los cargos/pagos salientes (negativos)
            if (rawAmount >= 0) continue;

            var absAmount = Math.Abs(rawAmount);
            var dateStr = cols[colDate].Trim();
            var timeStr = colTime >= 0 && colTime < cols.Count ? cols[colTime].Trim() : "";
            var txDate = ParseDateTime(dateStr, timeStr);

            var counterparty = colName >= 0 && colName < cols.Count ? cols[colName].Trim() : "PayPal";
            var currency = colCurrency >= 0 && colCurrency < cols.Count && !string.IsNullOrWhiteSpace(cols[colCurrency])
                ? cols[colCurrency].Trim().ToUpperInvariant()
                : "EUR";
            var txId = colTxId >= 0 && colTxId < cols.Count ? cols[colTxId].Trim() : Guid.NewGuid().ToString("N");
            var subject = colSubject >= 0 && colSubject < cols.Count ? cols[colSubject].Trim() : null;

            var order = new ExternalCommerceOrder
            {
                UserId = userId,
                Provider = CommerceProvider.PayPal,
                ExternalOrderId = txId,
                OrderDate = txDate,
                TotalAmount = absAmount,
                Currency = currency,
                CounterpartyName = counterparty
            };

            var itemTitle = !string.IsNullOrWhiteSpace(subject) ? subject : $"Pago a {counterparty}";
            var item = new ExternalCommerceOrderItem
            {
                ExternalCommerceOrderId = order.Id,
                Title = itemTitle,
                Quantity = 1,
                UnitPrice = absAmount,
                TotalPrice = absAmount
            };

            order.Items.Add(item);
            orders.Add(order);
        }

        return orders;
    }

    private static char DetectSeparator(string line)
    {
        if (line.Contains(';')) return ';';
        if (line.Contains('\t')) return '\t';
        return ',';
    }

    private static string NormalizeHeader(string text)
    {
        var clean = text.Trim().Trim('\uFEFF')
            .Replace("Ã¡", "a").Replace("Ã©", "e").Replace("Ã­", "i").Replace("Ã³", "o").Replace("Ãº", "u")
            .Replace("Ã ", "A").Replace("Ã‰", "E").Replace("Ã ", "I").Replace("Ã“", "O").Replace("Ãš", "U")
            .Replace("Ã±", "n").Replace("Ã‘", "N")
            .Replace("Ã", "I");

        var sb = new StringBuilder(clean.Length);
        foreach (var c in clean)
        {
            var upper = char.ToUpperInvariant(c);
            sb.Append(upper switch
            {
                'Á' => 'A',
                'É' => 'E',
                'Í' => 'I',
                'Ó' => 'O',
                'Ú' => 'U',
                'Ü' => 'U',
                _ => upper
            });
        }
        return sb.ToString();
    }

    private static int FindColumnIndex(List<string> headers, params string[] candidates)
    {
        for (int i = 0; i < headers.Count; i++)
        {
            var h = NormalizeHeader(headers[i]);
            foreach (var cand in candidates)
            {
                var normCand = NormalizeHeader(cand);
                if (h.Equals(normCand, StringComparison.OrdinalIgnoreCase) || h.Contains(normCand, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }
        }
        return -1;
    }

    private static List<string> ParseCsvLine(string line, char separator)
    {
        var result = new List<string>();
        var sb = new StringBuilder();
        var inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    sb.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == separator && !inQuotes)
            {
                result.Add(sb.ToString().Trim());
                sb.Clear();
            }
            else
            {
                sb.Append(c);
            }
        }
        result.Add(sb.ToString().Trim());
        return result;
    }

    private static DateTime ParseDateTime(string dateStr, string timeStr)
    {
        var combined = $"{dateStr} {timeStr}".Trim();
        if (DateTime.TryParse(combined, SpanishCulture, DateTimeStyles.None, out var dt)) return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
        if (DateTime.TryParse(combined, UsCulture, DateTimeStyles.None, out dt)) return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
        if (DateTime.TryParse(dateStr, SpanishCulture, DateTimeStyles.None, out dt)) return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
        if (DateTime.TryParse(dateStr, UsCulture, DateTimeStyles.None, out dt)) return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
        return DateTime.UtcNow;
    }

    private static decimal ParseSignedAmount(string val)
    {
        if (string.IsNullOrWhiteSpace(val)) return 0m;
        var clean = val.Replace("€", "").Replace("$", "").Replace("£", "").Replace(" ", "").Trim();

        // Si contiene tanto '.' como ','
        if (clean.Contains('.') && clean.Contains(','))
        {
            if (clean.LastIndexOf(',') > clean.LastIndexOf('.'))
            {
                // Formato europeo: -1.234,56 -> -1234.56
                clean = clean.Replace(".", "").Replace(",", ".");
            }
            else
            {
                // Formato anglosajón: -1,234.56 -> -1234.56
                clean = clean.Replace(",", "");
            }
        }
        else if (clean.Contains(','))
        {
            // Solo coma como separador decimal: -12,99 -> -12.99
            clean = clean.Replace(",", ".");
        }

        if (decimal.TryParse(clean, NumberStyles.Any, CultureInfo.InvariantCulture, out var amt))
        {
            return amt;
        }

        return 0m;
    }
}
