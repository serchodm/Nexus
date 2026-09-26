using System.Text.RegularExpressions;
using Nexus.Domain.Entities;
using Nexus.Domain.Enums;

namespace Nexus.Domain.Services;

/// <summary>
/// Motor de categorización en cascada que aplica la jerarquía de prevalencia estricta (RN-01).
/// </summary>
public static class CategorizationEngine
{
    public record CategorizationResult(
        Guid? CategoryId,
        Guid? SubcategoryId,
        CategorizationSource Source,
        bool IsRecurring,
        RecurrencePeriod RecurrencePeriod,
        ExpenseFlexibility ExpenseFlexibility,
        ExpenseNature ExpenseNature
    );

    /// <summary>
    /// Evalúa una transacción y determina su categoría y atributos contables según la jerarquía RN-01.
    /// </summary>
    public static CategorizationResult Evaluate(
        BankTransaction transaction,
        string? cleanMerchant,
        IEnumerable<CategorizationRule> userRules,
        IReadOnlyDictionary<string, Category>? systemCategoryLookup = null)
    {
        // RN-01: Si la transacción fue editada manualmente, está blindada contra sobreescrituras automáticas
        if (transaction.IsManual && transaction.CategoryId.HasValue)
        {
            return new CategorizationResult(
                transaction.CategoryId,
                transaction.SubcategoryId,
                CategorizationSource.Manual,
                transaction.IsRecurring,
                transaction.RecurrencePeriod,
                transaction.ExpenseFlexibility,
                transaction.ExpenseNature
            );
        }

        var description = transaction.Description;
        var merchant = cleanMerchant ?? string.Empty;

        // 1. Reglas de Usuario (Precedencia más alta después de Manual)
        foreach (var rule in userRules.Where(r => r.IsActive).OrderByDescending(r => r.Priority))
        {
            if (MatchesRule(rule, description, merchant))
            {
                return new CategorizationResult(
                    rule.TargetCategoryId,
                    rule.TargetSubcategoryId,
                    CategorizationSource.UserRule,
                    rule.SetIsRecurring ?? transaction.IsRecurring,
                    rule.SetIsRecurring == true ? RecurrencePeriod.Monthly : transaction.RecurrencePeriod,
                    rule.SetExpenseFlexibility ?? transaction.ExpenseFlexibility,
                    rule.SetExpenseNature ?? transaction.ExpenseNature
                );
            }
        }

        // 2. Semántica de Bizum y Transferencias
        var (platform, _) = ExternalPlatformDetector.Detect(description, transaction.BankMovementType);
        if (platform == ExternalPlatform.Bizum)
        {
            var bizumMatch = BizumConceptClassifier.Classify(description, transaction.Observations);
            if (bizumMatch.IsMatch && systemCategoryLookup != null)
            {
                var catKey = bizumMatch.CategoryName.ToUpperInvariant();
                var subKey = bizumMatch.SubcategoryName.ToUpperInvariant();

                Guid? catId = systemCategoryLookup.TryGetValue(catKey, out var cat) ? cat.Id : null;
                Guid? subId = systemCategoryLookup.TryGetValue(subKey, out var sub) ? sub.Id : null;

                if (catId.HasValue)
                {
                    return new CategorizationResult(
                        catId,
                        subId,
                        CategorizationSource.BizumSemantic,
                        false,
                        RecurrencePeriod.None,
                        ExpenseFlexibility.Discretionary,
                        ExpenseNature.Individual
                    );
                }
            }
        }

        // 3. Categoría Nativa del Banco (ej. extractos de ING Direct)
        if (!string.IsNullOrWhiteSpace(transaction.BankCategory) && systemCategoryLookup != null)
        {
            var bankCatUpper = transaction.BankCategory.ToUpperInvariant();
            if (systemCategoryLookup.TryGetValue(bankCatUpper, out var matchedBankCat))
            {
                Guid? subId = null;
                if (!string.IsNullOrWhiteSpace(transaction.BankSubcategory))
                {
                    var bankSubUpper = transaction.BankSubcategory.ToUpperInvariant();
                    if (systemCategoryLookup.TryGetValue(bankSubUpper, out var matchedSubCat))
                    {
                        subId = matchedSubCat.Id;
                    }
                }

                return new CategorizationResult(
                    matchedBankCat.Id,
                    subId,
                    CategorizationSource.BankNative,
                    transaction.IsRecurring,
                    transaction.RecurrencePeriod,
                    transaction.ExpenseFlexibility,
                    transaction.ExpenseNature
                );
            }
        }

        // 4. Reglas Heurísticas Globales del Sistema (Marcas comerciales y suscripciones)
        if (systemCategoryLookup != null)
        {
            var heuristic = EvaluateSystemHeuristics(description, merchant, systemCategoryLookup);
            if (heuristic != null)
            {
                return heuristic;
            }
        }

        // 5. Sin categorizar
        return new CategorizationResult(
            null,
            null,
            CategorizationSource.None,
            false,
            RecurrencePeriod.None,
            ExpenseFlexibility.Discretionary,
            transaction.ExpenseNature
        );
    }

    private static bool MatchesRule(CategorizationRule rule, string description, string cleanMerchant)
    {
        var targetText = $"{cleanMerchant} {description}".ToUpperInvariant();
        var pattern = (rule.NormalizedPattern ?? rule.Pattern).ToUpperInvariant();

        return rule.MatchType switch
        {
            RuleMatchType.Contains => targetText.Contains(pattern),
            RuleMatchType.StartsWith => targetText.StartsWith(pattern) || cleanMerchant.ToUpperInvariant().StartsWith(pattern),
            RuleMatchType.Exact => cleanMerchant.Equals(pattern, StringComparison.OrdinalIgnoreCase) || description.Equals(pattern, StringComparison.OrdinalIgnoreCase),
            RuleMatchType.Regex => Regex.IsMatch(targetText, rule.Pattern, RegexOptions.IgnoreCase),
            _ => targetText.Contains(pattern)
        };
    }

    private static CategorizationResult? EvaluateSystemHeuristics(
        string description, 
        string cleanMerchant, 
        IReadOnlyDictionary<string, Category> lookup)
    {
        var target = $"{cleanMerchant} {description}".ToUpperInvariant();

        // Supermercados y Alimentación
        if (Regex.IsMatch(target, @"\b(MERCADONA|CARREFOUR|LIDL|ALDI|DIA|ALCAMPO|EROSKI|CONSUM|SUPERCOR)\b"))
        {
            var cat = GetCategory(lookup, "ALIMENTACIÓN");
            var sub = GetCategory(lookup, "SUPERMERCADOS");
            return new CategorizationResult(cat?.Id, sub?.Id, CategorizationSource.SystemRule, false, RecurrencePeriod.None, ExpenseFlexibility.Discretionary, ExpenseNature.Shared);
        }

        // Streaming y Suscripciones Recurrentes
        if (Regex.IsMatch(target, @"\b(NETFLIX|SPOTIFY|HBO|DISNEY|PRIME VIDEO|APPLE\.COM\/BILL|YOUTUBE PREMIUM|AUDIBLE)\b"))
        {
            var cat = GetCategory(lookup, "SUSCRIPCIONES Y SERVICIOS");
            var sub = GetCategory(lookup, "STREAMING MULTIMEDIA");
            return new CategorizationResult(cat?.Id, sub?.Id, CategorizationSource.SystemRule, true, RecurrencePeriod.Monthly, ExpenseFlexibility.Fixed, ExpenseNature.Individual);
        }

        // VTC y Taxis
        if (Regex.IsMatch(target, @"\b(CABIFY|UBER|FREE NOW|BOLT|TAXI)\b"))
        {
            var cat = GetCategory(lookup, "TRANSPORTE");
            var sub = GetCategory(lookup, "TAXI Y VTC");
            return new CategorizationResult(cat?.Id, sub?.Id, CategorizationSource.SystemRule, false, RecurrencePeriod.None, ExpenseFlexibility.Discretionary, ExpenseNature.Individual);
        }

        // Combustible y Gasolineras
        if (Regex.IsMatch(target, @"\b(REPSOL|CEPSA|BP|GALP|SHELL|BALLENOIL|PLENOIL)\b"))
        {
            var cat = GetCategory(lookup, "TRANSPORTE");
            var sub = GetCategory(lookup, "COMBUSTIBLE");
            return new CategorizationResult(cat?.Id, sub?.Id, CategorizationSource.SystemRule, false, RecurrencePeriod.None, ExpenseFlexibility.Discretionary, ExpenseNature.Individual);
        }

        // Suministros del Hogar
        if (Regex.IsMatch(target, @"\b(IBERDROLA|ENDESA|NATURGY|TOTALENERGIES|REPSOL LUZ|CANAL DE ISABEL II|AGBAR)\b"))
        {
            var cat = GetCategory(lookup, "HOGAR Y VIVIENDA");
            var sub = GetCategory(lookup, "ELECTRICIDAD Y GAS");
            return new CategorizationResult(cat?.Id, sub?.Id, CategorizationSource.SystemRule, true, RecurrencePeriod.Monthly, ExpenseFlexibility.Fixed, ExpenseNature.Shared);
        }

        // Telecomunicaciones
        if (Regex.IsMatch(target, @"\b(VODAFONE|MOVISTAR|ORANGE|DIGI|YOIGO|MASMOVIL|O2|LOWI|PEPEPHONE)\b"))
        {
            var cat = GetCategory(lookup, "HOGAR Y VIVIENDA");
            var sub = GetCategory(lookup, "INTERNET Y TELÉFONO");
            return new CategorizationResult(cat?.Id, sub?.Id, CategorizationSource.SystemRule, true, RecurrencePeriod.Monthly, ExpenseFlexibility.Fixed, ExpenseNature.Shared);
        }

        return null;
    }

    private static Category? GetCategory(IReadOnlyDictionary<string, Category> lookup, string key)
    {
        return lookup.TryGetValue(key.ToUpperInvariant(), out var cat) ? cat : null;
    }
}
