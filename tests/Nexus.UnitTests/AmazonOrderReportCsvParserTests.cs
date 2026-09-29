using System.Text;
using Nexus.Application.Commerce.Parsers;
using Nexus.Domain.Enums;
using Xunit;

namespace Nexus.UnitTests;

public class AmazonOrderReportCsvParserTests
{
    private readonly AmazonOrderReportCsvParser _parser = new();

    [Fact]
    public async Task ParseAsync_SpanishHeaders_ParsesAndGroupsOrderItemsCorrectly()
    {
        // Arrange
        var csvContent = @"Número de pedido;Fecha del pedido;Título;ASIN/ISBN;Categoría;Cantidad;Precio unitario;Total de los artículos;Total general
405-1234567-8901234;2026-09-15;Ratón Inalámbrico Ergonómico;B08XYZ123;Electrónica;1;29,99 €;29,99 €;45,49 €
405-1234567-8901234;2026-09-15;Cable USB-C 2m;B09ABC456;Accesorios;1;15,50 €;15,50 €;45,49 €";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));
        var userId = Guid.NewGuid();

        // Act
        var orders = await _parser.ParseAsync(stream, userId);

        // Assert
        Assert.Single(orders);
        var order = orders[0];
        Assert.Equal("405-1234567-8901234", order.ExternalOrderId);
        Assert.Equal(CommerceProvider.Amazon, order.Provider);
        Assert.Equal(45.49m, order.TotalAmount);
        Assert.Equal(2, order.Items.Count);

        var mouse = order.Items.First(i => i.Sku == "B08XYZ123");
        Assert.Equal("Ratón Inalámbrico Ergonómico", mouse.Title);
        Assert.Equal(29.99m, mouse.TotalPrice);

        var cable = order.Items.First(i => i.Sku == "B09ABC456");
        Assert.Equal("Cable USB-C 2m", cable.Title);
        Assert.Equal(15.50m, cable.TotalPrice);
    }

    [Fact]
    public async Task ParseAsync_EnglishHeaders_CommaDelimited_ParsesSuccessfully()
    {
        // Arrange
        var csvContent = @"Order ID,Order Date,Title,ASIN,Category,Quantity,Unit Price,Item Total
111-9876543-2109876,2026-08-20,Kindle Paperwhite 16GB,B07TEST1,Devices,1,139.99,139.99";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));
        var userId = Guid.NewGuid();

        // Act
        var orders = await _parser.ParseAsync(stream, userId);

        // Assert
        Assert.Single(orders);
        var order = orders[0];
        Assert.Equal("111-9876543-2109876", order.ExternalOrderId);
        Assert.Equal(139.99m, order.TotalAmount);
        Assert.Single(order.Items);
        Assert.Equal("Kindle Paperwhite 16GB", order.Items.First().Title);
    }
}
