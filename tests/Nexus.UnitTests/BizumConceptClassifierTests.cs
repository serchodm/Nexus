using Nexus.Domain.Services;
using Xunit;

namespace Nexus.UnitTests;

public class BizumConceptClassifierTests
{
    [Theory]
    [InlineData("BIZUM CENA EL BODEGON", "Alimentación", "Restaurantes y Bares", true)]
    [InlineData("BIZUM PADEL MARTES", "Ocio y Estilo de Vida", "Hobbies y Deporte", true)]
    [InlineData("BIZUM GASOLINA VIAJE VALENCIA", "Transporte", "Combustible", true)]
    [InlineData("BIZUM ALQUILER SEPTIEMBRE", "Hogar y Vivienda", "Alquiler e Hipoteca", true)]
    [InlineData("BIZum regalo cumpleaños Carlos", "Ocio y Estilo de Vida", "Hobbies y Deporte", true)]
    [InlineData("BIZUM GRACIAS", "Finanzas e Ingresos", "Transferencias Recibidas", false)]
    public void Classify_ShouldMapIntentCorrectly(string concept, string expectedCat, string expectedSubcat, bool expectedMatch)
    {
        var result = BizumConceptClassifier.Classify(concept);

        Assert.Equal(expectedMatch, result.IsMatch);
        Assert.Equal(expectedCat, result.CategoryName);
        Assert.Equal(expectedSubcat, result.SubcategoryName);
        
        var expectedNature = expectedMatch 
            ? Nexus.Domain.Enums.TransactionNature.ExpenseReimbursement 
            : Nexus.Domain.Enums.TransactionNature.TrueIncome;
        Assert.Equal(expectedNature, result.SuggestedNature);
    }
}
