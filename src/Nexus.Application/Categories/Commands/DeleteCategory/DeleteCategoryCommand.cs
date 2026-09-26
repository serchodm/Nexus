using Nexus.Domain.Common;
using Nexus.Domain.Repositories;

namespace Nexus.Application.Categories.Commands.DeleteCategory;

public record DeleteCategoryCommand(Guid Id, Guid UserId);

public class DeleteCategoryCommandHandler
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(DeleteCategoryCommand cmd, CancellationToken ct = default)
    {
        var category = await _categoryRepository.GetByIdAsync(cmd.Id, ct);
        if (category == null)
        {
            return;
        }

        if (category.IsSystem || category.UserId == null)
        {
            throw new DomainValidationException("Las categorías predeterminadas del sistema no pueden eliminarse.");
        }

        if (category.UserId != cmd.UserId)
        {
            throw new DomainValidationException("No tienes permiso para eliminar esta categoría.");
        }

        _categoryRepository.Delete(category);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
