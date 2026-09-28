using System.IO.Compression;
using Microsoft.Extensions.Caching.Memory;
using Nexus.Application.Statements.Commands.PreviewStatement;
using Nexus.Application.Statements.Parsers;
using Nexus.Domain.Entities;
using Nexus.Domain.Enums;
using Nexus.UnitTests.Fakes;
using Xunit;

namespace Nexus.UnitTests;

public class PreviewStatementCommandHandlerTests
{
    private readonly FakeBankAccountRepository _accountRepository = new();
    private readonly FakeStatementImportRepository _importRepository = new();
    private readonly StatementParserFactory _parserFactory = new([new BbvaStatementParser(), new IngDirectStatementParser()]);
    private readonly FakeMemoryCache _cache = new();

    private PreviewStatementCommandHandler CreateHandler()
    {
        return new PreviewStatementCommandHandler(
            _accountRepository,
            _importRepository,
            _parserFactory,
            _cache);
    }

    private static byte[] CreateSampleBbvaXlsx()
    {
        using var ms = new MemoryStream();
        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
        {
            // [Content_Types].xml
            var entryTypes = archive.CreateEntry("[Content_Types].xml");
            using (var writer = new StreamWriter(entryTypes.Open()))
            {
                writer.Write("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\"><Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/><Default Extension=\"xml\" ContentType=\"application/xml\"/><Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/><Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/></Types>");
            }

            // _rels/.rels
            var entryRels = archive.CreateEntry("_rels/.rels");
            using (var writer = new StreamWriter(entryRels.Open()))
            {
                writer.Write("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/></Relationships>");
            }

            // xl/_rels/workbook.xml.rels
            var entryWbRels = archive.CreateEntry("xl/_rels/workbook.xml.rels");
            using (var writer = new StreamWriter(entryWbRels.Open()))
            {
                writer.Write("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\"/></Relationships>");
            }

            // xl/workbook.xml
            var entryWb = archive.CreateEntry("xl/workbook.xml");
            using (var writer = new StreamWriter(entryWb.Open()))
            {
                writer.Write("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\"><sheets><sheet name=\"Sheet1\" sheetId=\"1\" r:id=\"rId1\"/></sheets></workbook>");
            }

            // xl/worksheets/sheet1.xml
            var entrySheet = archive.CreateEntry("xl/worksheets/sheet1.xml");
            using (var writer = new StreamWriter(entrySheet.Open()))
            {
                writer.Write("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><sheetData><row r=\"1\"><c r=\"A1\" t=\"inlineStr\"><is><t>Fecha</t></is></c><c r=\"B1\" t=\"inlineStr\"><is><t>F.Valor</t></is></c><c r=\"C1\" t=\"inlineStr\"><is><t>Concepto</t></is></c><c r=\"D1\" t=\"inlineStr\"><is><t>Movimiento</t></is></c><c r=\"E1\" t=\"inlineStr\"><is><t>Importe</t></is></c><c r=\"F1\" t=\"inlineStr\"><is><t>Divisa</t></is></c><c r=\"G1\" t=\"inlineStr\"><is><t>Disponible</t></is></c><c r=\"H1\" t=\"inlineStr\"><is><t>Divisa</t></is></c><c r=\"I1\" t=\"inlineStr\"><is><t>Observaciones</t></is></c></row><row r=\"2\"><c r=\"A2\" t=\"inlineStr\"><is><t>25/09/2026</t></is></c><c r=\"B2\" t=\"inlineStr\"><is><t>25/09/2026</t></is></c><c r=\"C2\" t=\"inlineStr\"><is><t>PAGO EN COMERCIO MERCADONA</t></is></c><c r=\"D2\" t=\"inlineStr\"><is><t>COMPRA TARJETA</t></is></c><c r=\"E2\" t=\"inlineStr\"><is><t>-24,50</t></is></c><c r=\"F2\" t=\"inlineStr\"><is><t>EUR</t></is></c><c r=\"G2\" t=\"inlineStr\"><is><t>1.500,25</t></is></c><c r=\"H2\" t=\"inlineStr\"><is><t>EUR</t></is></c><c r=\"I2\" t=\"inlineStr\"><is><t>TARJETA 1234</t></is></c></row></sheetData></worksheet>");
            }
        }
        return ms.ToArray();
    }

    [Fact]
    public async Task HandleAsync_ValidBbvaXlsx_PreviewsSuccessfullyWithoutObjectDisposedException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var account = new BankAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "BBVA Nómina",
            Iban = "ES9101821234567890123456",
            BankCode = "0182",
            BankName = "BBVA",
            BankType = BankType.BBVA,
            Type = AccountType.Checking,
            HolderRole = HolderRole.Owner,
            OwnershipPercentage = 100
        };
        await _accountRepository.AddAsync(account);

        var xlsxBytes = CreateSampleBbvaXlsx();
        using var stream = new MemoryStream(xlsxBytes);

        var handler = CreateHandler();
        var cmd = new PreviewStatementCommand(userId, account.Id, "extracto_bbva.xlsx", stream);

        // Act
        var result = await handler.HandleAsync(cmd, CancellationToken.None);

        // Assert
        Assert.True(result.Success, result.ErrorMessage);
        Assert.NotNull(result.Preview);
        Assert.Equal("BBVA", result.Preview.DetectedBankName);
        Assert.Equal(1, result.Preview.TotalRecords);
        Assert.NotEqual(Guid.Empty, result.Preview.PreviewToken);
    }

    [Fact]
    public async Task ReadHeaderLineAsync_XlsxStream_LeavesStreamOpenWhenLeaveOpenIsTrue()
    {
        // Arrange
        var xlsxBytes = CreateSampleBbvaXlsx();
        using var stream = new MemoryStream(xlsxBytes);

        // Act
        var header = await StatementTableReader.ReadHeaderLineAsync(stream, "movimientos.xlsx");

        // Assert: stream should NOT be disposed and position can be reset
        Assert.True(stream.CanRead);
        Assert.True(stream.CanSeek);
        stream.Position = 0;

        var rows = await StatementTableReader.ReadRowsAsync(stream, "movimientos.xlsx");
        Assert.Equal(2, rows.Count);
    }
}
