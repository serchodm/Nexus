using System.Globalization;
using System.Text;
using Nexus.Domain.Entities;
using Nexus.Domain.Enums;

namespace Nexus.Application.Commerce.Parsers;

public class AmazonOrderReportCsvParser
{
    private static readonly CultureInfo SpanishCulture = CultureInfo.GetCultureInfo("es-ES");
    private static readonly CultureInfo UsCulture = CultureInfo.InvariantCulture;

    public async Task<List<ExternalCommerceOrder>> ParseAsync(
        Stream csvStream, 
        Guid userId, 
        CancellationToken ct = default)
    {
        using var reader = new StreamReader(csvStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        var ordersByExternalId = new Dictionary<string, ExternalCommerceOrder>(StringComparer.OrdinalIgnoreCase);

        string? headerLine = await reader.ReadLineAsync(ct);
        if (string.IsNullOrWhiteSpace(headerLine)) return new List<ExternalCommerceOrder>();

        var separator = DetectSeparator(headerLine);
        var headers = ParseCsvLine(headerLine, separator);

        var colOrderId = FindColumnIndex(headers, "ORDER ID", "NÚMERO DE PEDIDO", "NUMERO DE PEDIDO", "PEDIDO", "ORDER");
        var colOrderDate = FindColumnIndex(headers, "ORDER DATE", "FECHA DEL PEDIDO", "FECHA DE PEDIDO", "FECHA");
        var colShipDate = FindColumnIndex(headers, "SHIPMENT DATE", "FECHA DE ENVÍO", "FECHA DE ENVIO");
        var colTitle = FindColumnIndex(headers, "TITLE", "TÍTULO", "TITULO", "PRODUCT NAME", "NOMBRE DEL PRODUCTO", "ARTÍCULO", "ARTICULO", "DESCRIPCIÓN", "DESCRIPCION");
        var colCategory = FindColumnIndex(headers, "CATEGORY", "CATEGORÍA", "CATEGORIA");
        var colAsin = FindColumnIndex(headers, "ASIN/ISBN", "ASIN", "SKU");
        var colPrice = FindColumnIndex(headers, "ITEM TOTAL", "TOTAL DEL PRODUCTO", "TOTAL DE LOS ARTÍCULOS", "TOTAL DE LOS ARTICULOS", "TOTAL GENERAL", "PURCHASE PRICE PER UNIT", "PRECIO UNITARIO", "PRECIO", "TOTAL");
        var colQty = FindColumnIndex(headers, "QUANTITY", "CANTIDAD");
        var colCondition = FindColumnIndex(headers, "CONDITION", "ESTADO");
        var colCurrency = FindColumnIndex(headers, "CURRENCY", "DIVISA", "MONEDA");

        if (colOrderId < 0 && headers.Count > 0) colOrderId = 0;
        if (colTitle < 0 && headers.Count > 2) colTitle = 2;

        if (colOrderId < 0 || colTitle < 0)
        {
            throw new InvalidOperationException($"El formato del reporte de pedidos de Amazon no es válido (faltan columnas Order ID o Title). Headers recibidos ({headers.Count}): [{string.Join("|", headers)}], Separator: '{separator}', colOrderId={colOrderId}, colTitle={colTitle}");
        }

        string? line;
        while ((line = await reader.ReadLineAsync(ct)) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var cols = ParseCsvLine(line, separator);
            if (cols.Count <= Math.Max(colOrderId, colTitle)) continue;

            var externalOrderId = cols[colOrderId].Trim();
            if (string.IsNullOrWhiteSpace(externalOrderId)) continue;

            var title = cols[colTitle].Trim();
            if (string.IsNullOrWhiteSpace(title)) continue;

            var orderDate = colOrderDate >= 0 ? ParseDate(cols[colOrderDate]) : DateTime.UtcNow;
            var shipDate = colShipDate >= 0 ? ParseDateNullable(cols[colShipDate]) : null;
            var itemTotal = colPrice >= 0 ? ParseAmount(cols[colPrice]) : 0m;
            var qty = colQty >= 0 ? ParseInt(cols[colQty], 1) : 1;
            var asin = colAsin >= 0 && colAsin < cols.Count ? cols[colAsin].Trim() : null;
            var category = colCategory >= 0 && colCategory < cols.Count ? cols[colCategory].Trim() : null;
            var condition = colCondition >= 0 && colCondition < cols.Count ? cols[colCondition].Trim() : null;
            var currency = colCurrency >= 0 && colCurrency < cols.Count && !string.IsNullOrWhiteSpace(cols[colCurrency]) 
                ? cols[colCurrency].Trim().ToUpperInvariant() 
                : "EUR";

            if (!ordersByExternalId.TryGetValue(externalOrderId, out var order))
            {
                order = new ExternalCommerceOrder
                {
                    UserId = userId,
                    Provider = CommerceProvider.Amazon,
                    ExternalOrderId = externalOrderId,
                    OrderDate = orderDate,
                    ShipmentDate = shipDate,
                    Currency = currency,
                    CounterpartyName = "Amazon",
                    TotalAmount = 0m
                };
                ordersByExternalId[externalOrderId] = order;
            }

            // Actualizar fecha de expedición si este artículo tiene una más reciente
            if (shipDate.HasValue && (!order.ShipmentDate.HasValue || shipDate.Value > order.ShipmentDate.Value))
            {
                order.ShipmentDate = shipDate;
            }

            var item = new ExternalCommerceOrderItem
            {
                ExternalCommerceOrderId = order.Id,
                Title = title,
                Sku = asin,
                CategoryHint = category,
                Quantity = qty,
                UnitPrice = qty > 0 ? Math.Round(itemTotal / qty, 2) : itemTotal,
                TotalPrice = itemTotal,
                Condition = condition
            };

            order.Items.Add(item);
            order.TotalAmount += itemTotal;
        }

        return ordersByExternalId.Values.ToList();
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
            .Replace("Ã", "A").Replace("Ã", "E").Replace("Ã", "I").Replace("Ã", "O").Replace("Ã", "U")
            .Replace("Ã±", "n").Replace("Ã", "N")
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

    private static DateTime ParseDate(string val)
    {
        var clean = val.Trim();
        if (DateTime.TryParse(clean, SpanishCulture, DateTimeStyles.None, out var dt)) return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
        if (DateTime.TryParse(clean, UsCulture, DateTimeStyles.None, out dt)) return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
        return DateTime.UtcNow;
    }

    private static DateTime? ParseDateNullable(string val)
    {
        if (string.IsNullOrWhiteSpace(val)) return null;
        var clean = val.Trim();
        if (DateTime.TryParse(clean, SpanishCulture, DateTimeStyles.None, out var dt)) return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
        if (DateTime.TryParse(clean, UsCulture, DateTimeStyles.None, out dt)) return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
        return null;
    }

    private static decimal ParseAmount(string val)
    {
        if (string.IsNullOrWhiteSpace(val)) return 0m;
        var clean = val.Replace("€", "").Replace("$", "").Replace("£", "").Replace(" ", "").Trim();

        if (clean.Contains('.') && clean.Contains(','))
        {
            if (clean.LastIndexOf(',') > clean.LastIndexOf('.'))
            {
                clean = clean.Replace(".", "").Replace(",", ".");
            }
            else
            {
                clean = clean.Replace(",", "");
            }
        }
        else if (clean.Contains(','))
        {
            clean = clean.Replace(",", ".");
        }

        if (decimal.TryParse(clean, NumberStyles.Any, CultureInfo.InvariantCulture, out var amt))
        {
            return Math.Abs(amt);
        }

        return 0m;
    }

    private static int ParseInt(string val, int defaultValue = 1)
    {
        return int.TryParse(val.Trim(), out var parsed) ? Math.Max(1, parsed) : defaultValue;
    }
}
