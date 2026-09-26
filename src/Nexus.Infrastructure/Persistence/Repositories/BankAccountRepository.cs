using Microsoft.EntityFrameworkCore;
using Nexus.Domain.Entities;
using Nexus.Domain.Repositories;

namespace Nexus.Infrastructure.Persistence.Repositories;

public sealed class BankAccountRepository : IBankAccountRepository
{
    private readonly ApplicationDbContext _context;

    public BankAccountRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BankAccount?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.BankAccounts
            .FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task<List<BankAccount>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.BankAccounts
            .Where(a => a.UserId == userId)
            .OrderBy(a => a.Name)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsIbanForUserAsync(Guid userId, string iban, Guid? excludeAccountId = null, CancellationToken ct = default)
    {
        var cleanIban = iban.Trim().ToUpperInvariant();
        var query = _context.BankAccounts
            .Where(a => a.UserId == userId && a.Iban == cleanIban);

        if (excludeAccountId.HasValue)
        {
            query = query.Where(a => a.Id != excludeAccountId.Value);
        }

        return await query.AnyAsync(ct);
    }

    public async Task AddAsync(BankAccount account, CancellationToken ct = default)
    {
        await _context.BankAccounts.AddAsync(account, ct);
    }

    public void Update(BankAccount account)
    {
        _context.BankAccounts.Update(account);
    }
}
