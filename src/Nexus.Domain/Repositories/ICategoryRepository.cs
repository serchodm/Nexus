using Nexus.Domain.Entities;

namespace Nexus.Domain.Repositories;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Category>> GetAllSystemCategoriesAsync(CancellationToken ct = default);
    Task<List<Category>> GetCategoriesByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<List<Category>> GetCategoryTreeAsync(Guid userId, CancellationToken ct = default);
    Task<Dictionary<string, Category>> GetCategoryLookupAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(Category category, CancellationToken ct = default);
    void Update(Category category);
    void Delete(Category category);
}
