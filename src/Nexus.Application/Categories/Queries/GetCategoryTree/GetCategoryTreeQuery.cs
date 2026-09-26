using Nexus.Application.Categories.DTOs;
using Nexus.Domain.Repositories;

namespace Nexus.Application.Categories.Queries.GetCategoryTree;

public record GetCategoryTreeQuery(Guid UserId);

public class GetCategoryTreeQueryHandler
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoryTreeQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<List<CategoryTreeDto>> HandleAsync(GetCategoryTreeQuery query, CancellationToken ct = default)
    {
        var roots = await _categoryRepository.GetCategoryTreeAsync(query.UserId, ct);

        return roots.Select(r => new CategoryTreeDto(
            r.Id,
            r.Name,
            r.Icon,
            r.ColorHex,
            r.FlowType,
            r.DefaultExpenseNature,
            r.IsSystem,
            r.DisplayOrder,
            r.Subcategories.Select(s => new CategoryDto(
                s.Id,
                s.ParentCategoryId,
                s.Name,
                s.Icon,
                s.ColorHex,
                s.FlowType,
                s.DefaultExpenseNature,
                s.IsSystem,
                s.DisplayOrder
            )).ToList()
        )).ToList();
    }
}
