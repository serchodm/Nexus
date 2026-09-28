using System.Globalization;
using System.Text;
using ExcelDataReader;

namespace Nexus.Application.Statements.Parsers;

public static class StatementTableReader
{
    static StatementTableReader()
    {
        // Registrar proveedor de codificaciones para soportar Windows-1252 / ISO-8859-1 en exportaciones bancarias españolas
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    /// <summary>
    /// Lee un stream de extracto (Excel .xlsx/.xls o texto delimitado .csv/.tsv) y devuelve filas de columnas de texto limpio.
    /// </summary>
    public static async Task<List<string[]>> ReadRowsAsync(Stream stream, string fileName, CancellationToken ct = default)
    {
        var rows = new List<string[]>();
        var ext = Path.GetExtension(fileName).ToLowerInvariant();

        if (ext is ".xlsx" or ".xls")
        {
            using var reader = ExcelReaderFactory.CreateReader(stream, new ExcelReaderConfiguration
            {
                LeaveOpen = true
            });
            while (reader.Read())
            {
                var colCount = reader.FieldCount;
                var row = new string[colCount];
                var hasAnyContent = false;

                for (var i = 0; i < colCount; i++)
                {
                    var val = reader.GetValue(i);
                    var text = val switch
                    {
                        null => string.Empty,
                        DateTime dt => dt.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                        _ => val.ToString()?.Trim() ?? string.Empty
                    };

                    if (!string.IsNullOrWhiteSpace(text)) hasAnyContent = true;
                    row[i] = text;
                }

                if (hasAnyContent)
                {
                    rows.Add(row);
                }
            }
        }
        else
        {
            // Lectura de fichero de texto plano (TSV / CSV) con soporte para codificaciones ANSI / UTF-8
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream, ct);
            var bytes = memoryStream.ToArray();

            // Detectar codificación: verificar si es UTF-8 o Windows-1252
            Encoding encoding = DetectEncoding(bytes);

            using var reader = new StreamReader(new MemoryStream(bytes), encoding);
            string? line;
            char? separator = null;

            while ((line = await reader.ReadLineAsync(ct)) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                if (!separator.HasValue)
                {
                    separator = DetectSeparator(line);
                }

                var parts = line.Split(separator.Value)
                                .Select(p => p.Trim().Trim('"').Trim())
                                .ToArray();

                if (parts.Any(p => !string.IsNullOrWhiteSpace(p)))
                {
                    rows.Add(parts);
                }
            }
        }

        return rows;
    }

    public static async Task<string> ReadHeaderLineAsync(Stream stream, string fileName, CancellationToken ct = default)
    {
        var pos = stream.Position;
        try
        {
            var rows = await ReadRowsAsync(stream, fileName, ct);
            if (rows.Count > 0)
            {
                return string.Join(" | ", rows[0]);
            }
            return string.Empty;
        }
        finally
        {
            if (stream.CanSeek)
            {
                stream.Position = pos;
            }
        }
    }

    private static char DetectSeparator(string line)
    {
        if (line.Contains('\t')) return '\t';
        if (line.Contains(';')) return ';';
        if (line.Contains('|')) return '|';
        return ',';
    }

    private static Encoding DetectEncoding(byte[] bytes)
    {
        // Si tiene BOM UTF-8
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
        {
            return Encoding.UTF8;
        }

        // Intento UTF-8 estricto
        try
        {
            var utf8 = new UTF8Encoding(false, true);
            utf8.GetString(bytes);
            return Encoding.UTF8;
        }
        catch
        {
            // Fallback estándar para bancos españoles
            return Encoding.GetEncoding(1252); // Windows-1252
        }
    }
}
