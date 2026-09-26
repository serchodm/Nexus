using Nexus.Domain.Entities;
using Nexus.Domain.Enums;

namespace Nexus.Domain.Common;

/// <summary>
/// Catálogo canónico de categorías y subcategorías predeterminadas del sistema con identificadores fijos.
/// </summary>
public static class SystemCategoriesCatalog
{
    private static readonly DateTimeOffset SeedTimestamp = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public static readonly List<Category> AllCategories = new()
    {
        // 1. Alimentación
        CreateCategory(new Guid("00000000-0000-0000-0001-000000000000"), null, "Alimentación", "ALIMENTACION", "utensils", "#10B981", FlowType.Expense, ExpenseNature.Shared, 1),
        CreateCategory(new Guid("00000000-0000-0000-0001-000000000001"), new Guid("00000000-0000-0000-0001-000000000000"), "Supermercados", "SUPERMERCADOS", "shopping-cart", "#10B981", FlowType.Expense, ExpenseNature.Shared, 1),
        CreateCategory(new Guid("00000000-0000-0000-0001-000000000002"), new Guid("00000000-0000-0000-0001-000000000000"), "Restaurantes y Bares", "RESTAURANTES Y BARES", "utensils", "#10B981", FlowType.Expense, ExpenseNature.Shared, 2),
        CreateCategory(new Guid("00000000-0000-0000-0001-000000000003"), new Guid("00000000-0000-0000-0001-000000000000"), "Cafeterías", "CAFETERIAS", "coffee", "#10B981", FlowType.Expense, ExpenseNature.Individual, 3),
        CreateCategory(new Guid("00000000-0000-0000-0001-000000000004"), new Guid("00000000-0000-0000-0001-000000000000"), "Comida a Domicilio", "COMIDA A DOMICILIO", "bike", "#10B981", FlowType.Expense, ExpenseNature.Shared, 4),

        // 2. Hogar y Vivienda
        CreateCategory(new Guid("00000000-0000-0000-0002-000000000000"), null, "Hogar y Vivienda", "HOGAR Y VIVIENDA", "home", "#3B82F6", FlowType.Expense, ExpenseNature.Shared, 2),
        CreateCategory(new Guid("00000000-0000-0000-0002-000000000001"), new Guid("00000000-0000-0000-0002-000000000000"), "Alquiler e Hipoteca", "ALQUILER E HIPOTECA", "key", "#3B82F6", FlowType.Expense, ExpenseNature.Shared, 1),
        CreateCategory(new Guid("00000000-0000-0000-0002-000000000002"), new Guid("00000000-0000-0000-0002-000000000000"), "Electricidad y Gas", "ELECTRICIDAD Y GAS", "zap", "#3B82F6", FlowType.Expense, ExpenseNature.Shared, 2),
        CreateCategory(new Guid("00000000-0000-0000-0002-000000000003"), new Guid("00000000-0000-0000-0002-000000000000"), "Agua", "AGUA", "droplet", "#3B82F6", FlowType.Expense, ExpenseNature.Shared, 3),
        CreateCategory(new Guid("00000000-0000-0000-0002-000000000004"), new Guid("00000000-0000-0000-0002-000000000000"), "Internet y Teléfono", "INTERNET Y TELEFONO", "wifi", "#3B82F6", FlowType.Expense, ExpenseNature.Shared, 4),
        CreateCategory(new Guid("00000000-0000-0000-0002-000000000005"), new Guid("00000000-0000-0000-0002-000000000000"), "Menaje y Reformas", "MENAJE Y REFORMAS", "hammer", "#3B82F6", FlowType.Expense, ExpenseNature.Shared, 5),

        // 3. Transporte
        CreateCategory(new Guid("00000000-0000-0000-0003-000000000000"), null, "Transporte", "TRANSPORTE", "car", "#F59E0B", FlowType.Expense, ExpenseNature.Individual, 3),
        CreateCategory(new Guid("00000000-0000-0000-0003-000000000001"), new Guid("00000000-0000-0000-0003-000000000000"), "Combustible", "COMBUSTIBLE", "fuel", "#F59E0B", FlowType.Expense, ExpenseNature.Individual, 1),
        CreateCategory(new Guid("00000000-0000-0000-0003-000000000002"), new Guid("00000000-0000-0000-0003-000000000000"), "Transporte Público", "TRANSPORTE PUBLICO", "bus", "#F59E0B", FlowType.Expense, ExpenseNature.Individual, 2),
        CreateCategory(new Guid("00000000-0000-0000-0003-000000000003"), new Guid("00000000-0000-0000-0003-000000000000"), "Taxi y VTC", "TAXI Y VTC", "navigation", "#F59E0B", FlowType.Expense, ExpenseNature.Individual, 3),
        CreateCategory(new Guid("00000000-0000-0000-0003-000000000004"), new Guid("00000000-0000-0000-0003-000000000000"), "Parking y Peajes", "PARKING Y PEAJES", "circle-parking", "#F59E0B", FlowType.Expense, ExpenseNature.Individual, 4),
        CreateCategory(new Guid("00000000-0000-0000-0003-000000000005"), new Guid("00000000-0000-0000-0003-000000000000"), "Mantenimiento Vehículo", "MANTENIMIENTO VEHICULO", "wrench", "#F59E0B", FlowType.Expense, ExpenseNature.Individual, 5),

        // 4. Suscripciones y Servicios
        CreateCategory(new Guid("00000000-0000-0000-0004-000000000000"), null, "Suscripciones y Servicios", "SUSCRIPCIONES Y SERVICIOS", "tv", "#8B5CF6", FlowType.Expense, ExpenseNature.Individual, 4),
        CreateCategory(new Guid("00000000-0000-0000-0004-000000000001"), new Guid("00000000-0000-0000-0004-000000000000"), "Streaming Multimedia", "STREAMING MULTIMEDIA", "play", "#8B5CF6", FlowType.Expense, ExpenseNature.Individual, 1),
        CreateCategory(new Guid("00000000-0000-0000-0004-000000000002"), new Guid("00000000-0000-0000-0004-000000000000"), "Software y Nube", "SOFTWARE Y NUBE", "cloud", "#8B5CF6", FlowType.Expense, ExpenseNature.Individual, 2),
        CreateCategory(new Guid("00000000-0000-0000-0004-000000000003"), new Guid("00000000-0000-0000-0004-000000000000"), "Gimnasio y Membresías", "GIMNASIO Y MEMBRESIAS", "dumbbell", "#8B5CF6", FlowType.Expense, ExpenseNature.Individual, 3),

        // 5. Ocio y Estilo de Vida
        CreateCategory(new Guid("00000000-0000-0000-0005-000000000000"), null, "Ocio y Estilo de Vida", "OCIO Y ESTILO DE VIDA", "sparkles", "#EC4899", FlowType.Expense, ExpenseNature.Individual, 5),
        CreateCategory(new Guid("00000000-0000-0000-0005-000000000001"), new Guid("00000000-0000-0000-0005-000000000000"), "Viajes y Vacaciones", "VIAJES Y VACACIONES", "plane", "#EC4899", FlowType.Expense, ExpenseNature.Individual, 1),
        CreateCategory(new Guid("00000000-0000-0000-0005-000000000002"), new Guid("00000000-0000-0000-0005-000000000000"), "Cine y Teatro", "CINE Y TEATRO", "film", "#EC4899", FlowType.Expense, ExpenseNature.Individual, 2),
        CreateCategory(new Guid("00000000-0000-0000-0005-000000000003"), new Guid("00000000-0000-0000-0005-000000000000"), "Ropa y Calzado", "ROPA Y CALZADO", "shirt", "#EC4899", FlowType.Expense, ExpenseNature.Individual, 3),
        CreateCategory(new Guid("00000000-0000-0000-0005-000000000004"), new Guid("00000000-0000-0000-0005-000000000000"), "Hobbies y Deporte", "HOBBIES Y DEPORTE", "trophy", "#EC4899", FlowType.Expense, ExpenseNature.Individual, 4),

        // 6. Salud y Bienestar
        CreateCategory(new Guid("00000000-0000-0000-0006-000000000000"), null, "Salud y Bienestar", "SALUD Y BIENESTAR", "heart-pulse", "#06B6D4", FlowType.Expense, ExpenseNature.Individual, 6),
        CreateCategory(new Guid("00000000-0000-0000-0006-000000000001"), new Guid("00000000-0000-0000-0006-000000000000"), "Farmacia", "FARMACIA", "pill", "#06B6D4", FlowType.Expense, ExpenseNature.Individual, 1),
        CreateCategory(new Guid("00000000-0000-0000-0006-000000000002"), new Guid("00000000-0000-0000-0006-000000000000"), "Médicos y Dentista", "MEDICOS Y DENTISTA", "stethoscope", "#06B6D4", FlowType.Expense, ExpenseNature.Individual, 2),
        CreateCategory(new Guid("00000000-0000-0000-0006-000000000003"), new Guid("00000000-0000-0000-0006-000000000000"), "Óptica y Cuidado Personal", "OPTICA Y CUIDADO PERSONAL", "glasses", "#06B6D4", FlowType.Expense, ExpenseNature.Individual, 3),

        // 7. Finanzas e Ingresos
        CreateCategory(new Guid("00000000-0000-0000-0007-000000000000"), null, "Finanzas e Ingresos", "FINANZAS E INGRESOS", "wallet", "#22C55E", FlowType.Income, ExpenseNature.Individual, 7),
        CreateCategory(new Guid("00000000-0000-0000-0007-000000000001"), new Guid("00000000-0000-0000-0007-000000000000"), "Nómina y Salario", "NOMINA Y SALARIO", "banknote", "#22C55E", FlowType.Income, ExpenseNature.Individual, 1),
        CreateCategory(new Guid("00000000-0000-0000-0007-000000000002"), new Guid("00000000-0000-0000-0007-000000000000"), "Dividendos e Intereses", "DIVIDENDOS E INTERESES", "trending-up", "#22C55E", FlowType.Income, ExpenseNature.Individual, 2),
        CreateCategory(new Guid("00000000-0000-0000-0007-000000000003"), new Guid("00000000-0000-0000-0007-000000000000"), "Transferencias Recibidas", "TRANSFERENCIAS RECIBIDAS", "arrow-down-left", "#22C55E", FlowType.Income, ExpenseNature.Individual, 3),
        CreateCategory(new Guid("00000000-0000-0000-0007-000000000004"), new Guid("00000000-0000-0000-0007-000000000000"), "Devoluciones", "DEVOLUCIONES", "undo", "#22C55E", FlowType.Income, ExpenseNature.Individual, 4),

        // 8. Impuestos y Tasas
        CreateCategory(new Guid("00000000-0000-0000-0008-000000000000"), null, "Impuestos y Tasas", "IMPUESTOS Y TASAS", "receipt", "#64748B", FlowType.Expense, ExpenseNature.Individual, 8),
        CreateCategory(new Guid("00000000-0000-0000-0008-000000000001"), new Guid("00000000-0000-0000-0008-000000000000"), "IRPF", "IRPF", "file-text", "#64748B", FlowType.Expense, ExpenseNature.Individual, 1),
        CreateCategory(new Guid("00000000-0000-0000-0008-000000000002"), new Guid("00000000-0000-0000-0008-000000000000"), "IBI y Tasas Municipales", "IBI Y TASAS MUNICIPALES", "landmark", "#64748B", FlowType.Expense, ExpenseNature.Individual, 2),
        CreateCategory(new Guid("00000000-0000-0000-0008-000000000003"), new Guid("00000000-0000-0000-0008-000000000000"), "Seguros Generales", "SEGUROS GENERALES", "shield", "#64748B", FlowType.Expense, ExpenseNature.Individual, 3),
        CreateCategory(new Guid("00000000-0000-0000-0008-000000000004"), new Guid("00000000-0000-0000-0008-000000000000"), "Comisiones Bancarias", "COMISIONES BANCARIAS", "percent", "#64748B", FlowType.Expense, ExpenseNature.Individual, 4)
    };

    private static Category CreateCategory(
        Guid id, 
        Guid? parentId, 
        string name, 
        string normalizedName, 
        string icon, 
        string colorHex, 
        FlowType flowType, 
        ExpenseNature defaultNature, 
        int displayOrder)
    {
        return new Category
        {
            Id = id,
            UserId = null,
            ParentCategoryId = parentId,
            Name = name,
            NormalizedName = normalizedName,
            Icon = icon,
            ColorHex = colorHex,
            FlowType = flowType,
            DefaultExpenseNature = defaultNature,
            IsSystem = true,
            DisplayOrder = displayOrder,
            CreatedAt = SeedTimestamp,
            IsDeleted = false
        };
    }
}
