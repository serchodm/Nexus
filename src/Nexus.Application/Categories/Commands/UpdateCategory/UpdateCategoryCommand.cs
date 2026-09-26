using Nexus.Application.Categories.DTOs;
using Nexus.Domain.Common;
using Nexus.Domain.Enums;
using Nexus.Domain.Repositories;

namespace Nexus.Application.Categories.Commands.UpdateCategory;

public record UpdateCategoryCommand(
    Guid Id,
    Guid UserId,
    string Name,
    string Icon,
    string ColorHex,
    ExpenseNature DefaultExpenseNature,
    int DisplayOrder
);

public class UpdateCategoryCommandHandler
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CategoryDto> HandleAsync(UpdateCategoryCommand cmd, CancellationToken ct = default)
    {
        var category = await _categoryRepository.GetByIdAsync(cmd.Id, ct);
        if (category == null)
        {
            throw new DomainValidationException("La categoría no existe.");
        }

        if (category.IsSystem || category.UserId == null)
        {
            throw new DomainValidationException("Las categorías predeterminadas del sistema son de solo lectura y no pueden modificarse.");
        }

        if (category.UserId != cmd.UserId)
        {
            throw new DomainValidationException("No tienes permiso para modificar esta categoría.");
        }

        if (string.IsNullOrWhiteSpace(cmd.Name))
        {
            throw new DomainValidationException("El nombre de la categoría es obligatorio.");
        }

        category.Name = cmd.Name.Trim();
        category.NormalizedName = cmd.Name.Trim().ToUpperInvariant();
        category.Icon = string.IsNullOrWhiteSpace(cmd.Icon) ? category.Icon : cmd.Icon.Trim();
        category.ColorHex = string.IsNullOrWhiteSpace(cmd.ColorHex) ? category.ColorHex : cmd.ColorHex.Trim();
        category.DefaultExpenseNature = cmd.DefaultExpenseNature;
        category.DisplayOrder = cmd.DisplayOrder;

        _categoryRepository.Update(category);
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
