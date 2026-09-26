using System.Text;
using Nexus.Application.Statements.Parsers;
using Nexus.Domain.Enums;
using Xunit;

namespace Nexus.UnitTests;

public class BbvaStatementParserTests
{
    private readonly BbvaStatementParser _parser = new();

    [Fact]
    public void CanParse_ValidBbvaHeader_ReturnsTrue()
    {
        var header = "Fecha\tF.Valor\tConcepto\tMovimiento\tImporte\tDivisa\tDisponible\tDivisa\tObservaciones";
        Assert.True(_parser.CanParse(header));
    }

    [Fact]
    public async Task ParseAsync_BbvaTsv_ParsesAndOrdersChronologicallyAscending()
    {
        // BBVA exports with newest on top (25/09 before 24/09)
        var content = 
            "Fecha\tF.Valor\tConcepto\tMovimiento\tImporte\tDivisa\tDisponible\tDivisa\tObservaciones\n" +
            "25/09/2026\t25/09/2026\tPAGO EN COMERCIO MERCADONA\tCOMPRA TARJETA\t-24,50\tEUR\t1.500,25\tEUR\tPAGO TARJETA 4188123456786589\n" +
            "24/09/2026\t24/09/2026\tNOMINA EMPRESA SL\tTRANSFERENCIA\t1.850,00\tEUR\t1.524,75\tEUR\tTRANSFERENCIA RECIBIDA";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var result = await _parser.ParseAsync(stream, "movimientos.tsv", CancellationToken.None);

        Assert.Equal(BankType.BBVA, result.BankType);
        Assert.Equal(2, result.Transactions.Count);

        // First item chronologically should be 24/09
        Assert.Equal(new DateOnly(2026, 9, 24), result.Transactions[0].OperationDate);
        Assert.Equal(1850.00m, result.Transactions[0].Amount);
        Assert.Equal(1524.75m, result.Transactions[0].BalanceAfter);

        // Second item chronologically should be 25/09
        Assert.Equal(new DateOnly(2026, 9, 25), result.Transactions[1].OperationDate);
        Assert.Equal(-24.50m, result.Transactions[1].Amount);
        Assert.Equal(1500.25m, result.Transactions[1].BalanceAfter);

        // Detected card: 6589
        Assert.Contains("6589", result.DetectedCards);

        // Sanitized description & observations (PAN masked)
        Assert.DoesNotContain("4188123456786589", result.Transactions[1].Observations);
        Assert.Contains("**** **** **** 6589", result.Transactions[1].Observations);

        // Ending balance is 1500.25 (after 25/09)
        Assert.Equal(1500.25m, result.EndingBalance);
    }
}
