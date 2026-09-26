using Nexus.Domain.Enums;

namespace Nexus.Application.Categories.DTOs;

public record CategoryDto(
    Guid Id,
    Guid? ParentCategoryId,
    string Name,
    string Icon,
    string ColorHex,
    FlowType FlowType,
    ExpenseNature DefaultExpenseNature,
    bool IsSystem,
    int DisplayOrder
);

public record CategoryTreeDto(
    Guid Id,
    string Name,
    string Icon,
    string ColorHex,
    FlowType FlowType,
    ExpenseNature DefaultExpenseNature,
    bool IsSystem,
    int DisplayOrder,
    List<CategoryDto> Subcategories
);
