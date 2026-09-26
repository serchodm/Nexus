using Microsoft.EntityFrameworkCore;
using Nexus.Domain.Entities;
using Nexus.Domain.Repositories;

namespace Nexus.Infrastructure.Persistence.Repositories;

public sealed class StatementImportRepository : IStatementImportRepository
{
    private readonly ApplicationDbContext _context;

    public StatementImportRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StatementImport?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.StatementImports
            .Include(i => i.BankAccount)
            .FirstOrDefaultAsync(i => i.Id == id, ct);
    }

    public async Task<bool> ExistsHashForAccountAsync(Guid accountId, string fileHashSha256, CancellationToken ct = default)
    {
        return await _context.StatementImports
            .AnyAsync(i => i.BankAccountId == accountId && i.FileHashSha256 == fileHashSha256, ct);
    }

    public async Task<List<StatementImport>> GetByAccountIdAsync(Guid accountId, CancellationToken ct = default)
    {
        return await _context.StatementImports
            .Where(i => i.BankAccountId == accountId)
            .OrderByDescending(i => i.ImportedAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(StatementImport import, CancellationToken ct = default)
    {
        await _context.StatementImports.AddAsync(import, ct);
    }

    public void Remove(StatementImport import)
    {
        _context.StatementImports.Remove(import);
    }
}
