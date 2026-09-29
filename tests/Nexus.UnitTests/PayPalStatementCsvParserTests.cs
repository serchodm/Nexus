using System.Text;
using Nexus.Application.Commerce.Parsers;
using Nexus.Domain.Enums;
using Xunit;

namespace Nexus.UnitTests;

public class PayPalStatementCsvParserTests
{
    private readonly PayPalStatementCsvParser _parser = new();

    [Fact]
    public async Task ParseAsync_SpanishHeaders_ParsesExpenseAndExtractsCounterparty()
    {
        // Arrange
        var csvContent = @"Fecha;Hora;Zona horaria;Nombre;Tipo;Estado;Divisa;Bruto;Comisión;Neto;Número de transacción
14/09/2026;18:30:12;CEST;Spotify AB;Pago exprés;Completado;EUR;-12,99;0,00;-12,99;9XX123456789
15/09/2026;10:15:00;CEST;Amigo Transferencia;Transferencia recibida;Completado;EUR;50,00;0,00;50,00;8YY987654321";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));
        var userId = Guid.NewGuid();

        // Act
        var orders = await _parser.ParseAsync(stream, userId);

        // Assert: solo debe importar el cargo negativo (-12.99), ignorando el ingreso positivo (+50.00)
        Assert.Single(orders);
        var order = orders[0];
        Assert.Equal("9XX123456789", order.ExternalOrderId);
        Assert.Equal(CommerceProvider.PayPal, order.Provider);
        Assert.Equal(12.99m, order.TotalAmount);
        Assert.Equal("Spotify AB", order.CounterpartyName);
        Assert.Single(order.Items);
        Assert.Contains("Spotify AB", order.Items.First().Title);
    }

    [Fact]
    public async Task ParseAsync_EnglishHeaders_ParsesCorrectly()
    {
        // Arrange
        var csvContent = @"Date,Time,TimeZone,Name,Type,Status,Currency,Gross,Fee,Net,Transaction ID
09/10/2026,14:22:00,GMT,Uber BV,Express Checkout,Completed,EUR,-24.50,0.00,-24.50,7ZZ456789123";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));
        var userId = Guid.NewGuid();

        // Act
        var orders = await _parser.ParseAsync(stream, userId);

        // Assert
        Assert.Single(orders);
        Assert.Equal("7ZZ456789123", orders[0].ExternalOrderId);
        Assert.Equal(24.50m, orders[0].TotalAmount);
        Assert.Equal("Uber BV", orders[0].CounterpartyName);
    }
}
