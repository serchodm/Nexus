using Microsoft.EntityFrameworkCore;
using Nexus.Domain.Entities;
using Nexus.Domain.Enums;
using Nexus.Domain.Repositories;

namespace Nexus.Infrastructure.Persistence.Repositories;

public sealed class BankConnectionRepository : IBankConnectionRepository
{
    private readonly ApplicationDbContext _context;

    public BankConnectionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BankConnection?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.BankConnections
            .Include(c => c.Accounts)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<BankConnection?> GetBySessionIdAsync(string externalSessionId, CancellationToken ct = default)
    {
        return await _context.BankConnections
            .Include(c => c.Accounts)
            .FirstOrDefaultAsync(c => c.ExternalSessionId == externalSessionId, ct);
    }

    public async Task<List<BankConnection>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.BankConnections
            .Include(c => c.Accounts)
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<BankConnection>> GetActiveConnectionsAsync(CancellationToken ct = default)
    {
        return await _context.BankConnections
            .Include(c => c.Accounts)
            .Where(c => c.Status == BankConnectionStatus.Active || c.Status == BankConnectionStatus.ExpiringSoon)
            .ToListAsync(ct);
    }

    public async Task AddAsync(BankConnection connection, CancellationToken ct = default)
    {
        await _context.BankConnections.AddAsync(connection, ct);
    }

    public void Update(BankConnection connection)
    {
        _context.BankConnections.Update(connection);
    }

    public void Delete(BankConnection connection)
    {
        _context.BankConnections.Remove(connection);
    }
}
