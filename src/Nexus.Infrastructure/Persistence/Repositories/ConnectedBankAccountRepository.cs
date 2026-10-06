using Microsoft.EntityFrameworkCore;
using Nexus.Domain.Entities;
using Nexus.Domain.Repositories;

namespace Nexus.Infrastructure.Persistence.Repositories;

public sealed class ConnectedBankAccountRepository : IConnectedBankAccountRepository
{
    private readonly ApplicationDbContext _context;

    public ConnectedBankAccountRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ConnectedBankAccount?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ConnectedBankAccounts
            .Include(a => a.BankConnection)
            .FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task<ConnectedBankAccount?> GetByExternalAccountIdAsync(string externalAccountId, CancellationToken ct = default)
    {
        return await _context.ConnectedBankAccounts
            .Include(a => a.BankConnection)
            .FirstOrDefaultAsync(a => a.ExternalAccountId == externalAccountId, ct);
    }

    public async Task<List<ConnectedBankAccount>> GetByBankConnectionIdAsync(Guid bankConnectionId, CancellationToken ct = default)
    {
        return await _context.ConnectedBankAccounts
            .Where(a => a.BankConnectionId == bankConnectionId)
            .ToListAsync(ct);
    }

    public async Task AddAsync(ConnectedBankAccount account, CancellationToken ct = default)
    {
        await _context.ConnectedBankAccounts.AddAsync(account, ct);
    }

    public void Update(ConnectedBankAccount account)
    {
        _context.ConnectedBankAccounts.Update(account);
    }

    public void Delete(ConnectedBankAccount account)
    {
        _context.ConnectedBankAccounts.Remove(account);
    }
}
