using Nexus.Application.Categories.DTOs;
using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Nexus.Domain.Enums;
using Nexus.Domain.Repositories;

namespace Nexus.Application.Categories.Commands.CreateCategory;

public record CreateCategoryCommand(
    Guid UserId,
    Guid? ParentCategoryId,
    string Name,
    string Icon,
    string ColorHex,
    FlowType FlowType,
    ExpenseNature DefaultExpenseNature,
    int DisplayOrder = 0
);

public class CreateCategoryCommandHandler
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CategoryDto> HandleAsync(CreateCategoryCommand cmd, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(cmd.Name))
        {
            throw new DomainValidationException("El nombre de la categoría es obligatorio.");
        }

        if (cmd.ParentCategoryId.HasValue)
        {
            var parent = await _categoryRepository.GetByIdAsync(cmd.ParentCategoryId.Value, ct);
            if (parent == null || (parent.UserId != null && parent.UserId != cmd.UserId))
            {
                throw new DomainValidationException("La categoría padre seleccionada no existe o no tienes acceso a ella.");
            }
        }

        var normalizedName = cmd.Name.Trim().ToUpperInvariant();

        var category = new Category
        {
            UserId = cmd.UserId,
            ParentCategoryId = cmd.ParentCategoryId,
            Name = cmd.Name.Trim(),
            NormalizedName = normalizedName,
            Icon = string.IsNullOrWhiteSpace(cmd.Icon) ? "tag" : cmd.Icon.Trim(),
            ColorHex = string.IsNullOrWhiteSpace(cmd.ColorHex) ? "#64748B" : cmd.ColorHex.Trim(),
            FlowType = cmd.FlowType,
            DefaultExpenseNature = cmd.DefaultExpenseNature,
            IsSystem = false,
            DisplayOrder = cmd.DisplayOrder
        };

        await _categoryRepository.AddAsync(category, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new CategoryDto(
            category.Id,
            category.ParentCategoryId,
            category.Name,
            category.Icon,
            category.ColorHex,
            category.FlowType,
            category.DefaultExpenseNature,
            category.IsSystem,
            category.DisplayOrder
        );
    }
}
