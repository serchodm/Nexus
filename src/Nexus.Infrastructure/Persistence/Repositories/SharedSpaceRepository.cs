using Microsoft.EntityFrameworkCore;
using Nexus.Domain.Entities;
using Nexus.Domain.Repositories;

namespace Nexus.Infrastructure.Persistence.Repositories;

public sealed class SharedSpaceRepository : ISharedSpaceRepository
{
    private readonly ApplicationDbContext _context;

    public SharedSpaceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SharedSpace?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.SharedSpaces
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<SharedSpace?> GetByIdWithMembersAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.SharedSpaces
            .Include(s => s.Members)
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<SharedSpace?> GetActiveForUserAsync(Guid userId, CancellationToken ct = default)
    {
        // Busca espacio donde el usuario sea miembro activo o retorna el primer espacio activo (soporte demo/local)
        var space = await _context.SharedSpaces
            .Include(s => s.Members)
            .FirstOrDefaultAsync(s => s.Members.Any(m => m.UserId == userId && m.IsActive), ct);

        if (space == null)
        {
            space = await _context.SharedSpaces
                .Include(s => s.Members)
                .FirstOrDefaultAsync(ct);
        }

        return space;
    }

    public async Task AddAsync(SharedSpace space, CancellationToken ct = default)
    {
        await _context.SharedSpaces.AddAsync(space, ct);
    }

    public void Update(SharedSpace space)
    {
        _context.SharedSpaces.Update(space);
    }
}
