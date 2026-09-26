using Nexus.Domain.Entities;
using Nexus.Domain.Enums;
using Nexus.Domain.Services;
using Xunit;

namespace Nexus.UnitTests;

public class CategorizationEngineTests
{
    private readonly Dictionary<string, Category> _systemLookup;
    private readonly Category _catAlimentacion;
    private readonly Category _subSupermercados;
    private readonly Category _catSuscripciones;
    private readonly Category _subStreaming;
    private readonly Category _catTransporte;
    private readonly Category _subTaxi;

    public CategorizationEngineTests()
    {
        _catAlimentacion = new Category { Id = Guid.NewGuid(), Name = "Alimentación", NormalizedName = "ALIMENTACION" };
        _subSupermercados = new Category { Id = Guid.NewGuid(), Name = "Supermercados", NormalizedName = "SUPERMERCADOS", ParentCategoryId = _catAlimentacion.Id };

        _catSuscripciones = new Category { Id = Guid.NewGuid(), Name = "Suscripciones y Servicios", NormalizedName = "SUSCRIPCIONES Y SERVICIOS" };
        _subStreaming = new Category { Id = Guid.NewGuid(), Name = "Streaming Multimedia", NormalizedName = "STREAMING MULTIMEDIA", ParentCategoryId = _catSuscripciones.Id };

        _catTransporte = new Category { Id = Guid.NewGuid(), Name = "Transporte", NormalizedName = "TRANSPORTE" };
        _subTaxi = new Category { Id = Guid.NewGuid(), Name = "Taxi y VTC", NormalizedName = "TAXI Y VTC", ParentCategoryId = _catTransporte.Id };

        _systemLookup = new Dictionary<string, Category>(StringComparer.OrdinalIgnoreCase)
        {
            ["ALIMENTACIÓN"] = _catAlimentacion,
            ["ALIMENTACION"] = _catAlimentacion,
            ["SUPERMERCADOS"] = _subSupermercados,
            ["SUSCRIPCIONES Y SERVICIOS"] = _catSuscripciones,
            ["STREAMING MULTIMEDIA"] = _subStreaming,
            ["TRANSPORTE"] = _catTransporte,
            ["TAXI Y VTC"] = _subTaxi
        };
    }

    [Fact]
    public void Evaluate_WhenTransactionIsManual_ShouldNeverBeOverridden()
    {
        var existingCatId = Guid.NewGuid();
        var tx = new BankTransaction
        {
            Description = "MERCADONA COMPRA",
            IsManual = true,
            CategoryId = existingCatId,
            CategorizationSource = CategorizationSource.Manual
        };

        var result = CategorizationEngine.Evaluate(tx, "Mercadona", Enumerable.Empty<CategorizationRule>(), _systemLookup);

        Assert.Equal(existingCatId, result.CategoryId);
        Assert.Equal(CategorizationSource.Manual, result.Source);
    }

    [Fact]
    public void Evaluate_WhenUserRuleMatches_ShouldTakePrecedenceOverSystemRule()
    {
        var userTargetCatId = Guid.NewGuid();
        var userRule = new CategorizationRule
        {
            Pattern = "MERCADONA",
            TargetCategoryId = userTargetCatId,
            Priority = 50,
            IsActive = true
        };

        var tx = new BankTransaction
        {
            Description = "COMPRA TPV MERCADONA"
        };

        var result = CategorizationEngine.Evaluate(tx, "Mercadona", new[] { userRule }, _systemLookup);

        Assert.Equal(userTargetCatId, result.CategoryId);
        Assert.Equal(CategorizationSource.UserRule, result.Source);
    }

    [Fact]
    public void Evaluate_WhenSystemHeuristicMatchesSubscription_ShouldSetRecurringAndFixed()
    {
        var tx = new BankTransaction
        {
            Description = "PAYPAL *NETFLIX 9283"
        };

        var result = CategorizationEngine.Evaluate(tx, "Netflix", Enumerable.Empty<CategorizationRule>(), _systemLookup);

        Assert.Equal(_catSuscripciones.Id, result.CategoryId);
        Assert.Equal(_subStreaming.Id, result.SubcategoryId);
        Assert.Equal(CategorizationSource.SystemRule, result.Source);
        Assert.True(result.IsRecurring);
        Assert.Equal(RecurrencePeriod.Monthly, result.RecurrencePeriod);
        Assert.Equal(ExpenseFlexibility.Fixed, result.ExpenseFlexibility);
    }
}
