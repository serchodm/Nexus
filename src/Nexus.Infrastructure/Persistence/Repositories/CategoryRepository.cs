using Microsoft.EntityFrameworkCore;
using Nexus.Domain.Entities;
using Nexus.Domain.Repositories;

namespace Nexus.Infrastructure.Persistence.Repositories;

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _context;

    public CategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Categories
            .Include(c => c.Subcategories)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<List<Category>> GetAllSystemCategoriesAsync(CancellationToken ct = default)
    {
        return await _context.Categories
            .AsNoTracking()
            .Where(c => c.IsSystem && c.UserId == null)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(ct);
    }

    public async Task<List<Category>> GetCategoriesByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.Categories
            .AsNoTracking()
            .Where(c => c.UserId == null || c.UserId == userId)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(ct);
    }

    public async Task<List<Category>> GetCategoryTreeAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.Categories
            .AsNoTracking()
            .Include(c => c.Subcategories.OrderBy(s => s.DisplayOrder))
            .Where(c => (c.UserId == null || c.UserId == userId) && c.ParentCategoryId == null)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(ct);
    }

    public async Task<Dictionary<string, Category>> GetCategoryLookupAsync(Guid userId, CancellationToken ct = default)
    {
        var all = await _context.Categories
            .AsNoTracking()
            .Where(c => c.UserId == null || c.UserId == userId)
            .ToListAsync(ct);

        var lookup = new Dictionary<string, Category>(StringComparer.OrdinalIgnoreCase);

        foreach (var cat in all)
        {
            if (!string.IsNullOrWhiteSpace(cat.NormalizedName))
            {
                lookup[cat.NormalizedName] = cat;
            }
            if (!string.IsNullOrWhiteSpace(cat.Name))
            {
                lookup[cat.Name] = cat;
            }
        }

        return lookup;
    }

    public async Task AddAsync(Category category, CancellationToken ct = default)
    {
        await _context.Categories.AddAsync(category, ct);
    }

    public void Update(Category category)
    {
        _context.Categories.Update(category);
    }

    public void Delete(Category category)
    {
        _context.Categories.Remove(category);
    }
}
