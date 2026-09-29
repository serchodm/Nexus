using Microsoft.EntityFrameworkCore;
using Nexus.Domain.Entities;
using Nexus.Domain.Enums;
using Nexus.Domain.Repositories;

namespace Nexus.Infrastructure.Persistence.Repositories;

public sealed class ExternalCommerceConnectionRepository : IExternalCommerceConnectionRepository
{
    private readonly ApplicationDbContext _context;

    public ExternalCommerceConnectionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ExternalCommerceConnection?> GetByUserIdAndProviderAsync(
        Guid userId, 
        CommerceProvider provider, 
        CancellationToken ct = default)
    {
        return await _context.ExternalCommerceConnections
            .FirstOrDefaultAsync(c => c.UserId == userId && c.Provider == provider, ct);
    }

    public async Task<List<ExternalCommerceConnection>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.ExternalCommerceConnections
            .Where(c => c.UserId == userId)
            .ToListAsync(ct);
    }

    public async Task AddAsync(ExternalCommerceConnection connection, CancellationToken ct = default)
    {
        await _context.ExternalCommerceConnections.AddAsync(connection, ct);
    }

    public void Update(ExternalCommerceConnection connection)
    {
        _context.ExternalCommerceConnections.Update(connection);
    }
}
