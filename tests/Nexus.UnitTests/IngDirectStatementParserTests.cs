using System.Text;
using Nexus.Application.Statements.Parsers;
using Nexus.Domain.Enums;
using Xunit;

namespace Nexus.UnitTests;

public class IngDirectStatementParserTests
{
    private readonly IngDirectStatementParser _parser = new();

    [Fact]
    public void CanParse_ValidIngHeader_ReturnsTrue()
    {
        var header = "F. VALOR\tF. OPERACIÓN\tCATEGORÍA\tSUBCATEGORÍA\tDESCRIPCIÓN\tIMPORTE (€)\tSALDO (€)\tCOMENTARIO";
        Assert.True(_parser.CanParse(header));
    }

    [Fact]
    public async Task ParseAsync_IngTsv_ParsesAndPreservesNativeCategories()
    {
        // Descending order from bank export
        var content = 
            "F. VALOR\tF. OPERACIÓN\tCATEGORÍA\tSUBCATEGORÍA\tDESCRIPCIÓN\tIMPORTE (€)\tSALDO (€)\tCOMENTARIO\n" +
            "25/09/2026\t25/09/2026\tOcio y viajes\tCafeterías y restaurantes\tCAFETERIA STARBUCKS\t-4,50\t2.150,00\tDesayuno equipo\n" +
            "24/09/2026\t24/09/2026\tAlimentación\tSupermercados\tMERCADONA\t-45,20\t2.154,50\tCompra semanal";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var result = await _parser.ParseAsync(stream, "extracto_ing.tsv", CancellationToken.None);

        Assert.Equal(BankType.ING, result.BankType);
        Assert.Equal(2, result.Transactions.Count);

        // First item chronologically should be 24/09
        Assert.Equal(new DateOnly(2026, 9, 24), result.Transactions[0].OperationDate);
        Assert.Equal("Alimentación", result.Transactions[0].BankCategory);
        Assert.Equal("Supermercados", result.Transactions[0].BankSubcategory);
        Assert.Equal(-45.20m, result.Transactions[0].Amount);
        Assert.Equal(2154.50m, result.Transactions[0].BalanceAfter);

        // Second item chronologically should be 25/09
        Assert.Equal(new DateOnly(2026, 9, 25), result.Transactions[1].OperationDate);
        Assert.Equal("Ocio y viajes", result.Transactions[1].BankCategory);
        Assert.Equal("Cafeterías y restaurantes", result.Transactions[1].BankSubcategory);
        Assert.Equal(-4.50m, result.Transactions[1].Amount);
        Assert.Equal(2150.00m, result.Transactions[1].BalanceAfter);

        Assert.Equal(2150.00m, result.EndingBalance);
    }
}
