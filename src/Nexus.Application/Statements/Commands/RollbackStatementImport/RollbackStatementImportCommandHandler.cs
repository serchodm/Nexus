using Nexus.Domain.Common;
using Nexus.Domain.Repositories;

namespace Nexus.Application.Statements.Commands.RollbackStatementImport;

public class RollbackStatementImportCommandHandler
{
    private readonly IStatementImportRepository _importRepository;
    private readonly IBankAccountRepository _accountRepository;
    private readonly IBankTransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RollbackStatementImportCommandHandler(
        IStatementImportRepository importRepository,
        IBankAccountRepository accountRepository,
        IBankTransactionRepository transactionRepository,
        IUnitOfWork unitOfWork)
    {
        _importRepository = importRepository;
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<RollbackStatementImportResult> HandleAsync(RollbackStatementImportCommand cmd, CancellationToken ct = default)
    {
        // 1. Obtener el lote de importación
        var import = await _importRepository.GetByIdAsync(cmd.ImportId, ct);
        if (import == null)
        {
            return new RollbackStatementImportResult(false, "El lote de importación no existe.");
        }

        // 2. Validar que la cuenta pertenezca al usuario
        var account = await _accountRepository.GetByIdAsync(import.BankAccountId, ct);
        if (account == null || account.UserId != cmd.UserId)
        {
            return new RollbackStatementImportResult(false, "No tienes permiso para deshacer esta importación.");
        }

        // 3. Eliminar transacciones asociadas y el lote de importación (soft-delete)
        await _transactionRepository.DeleteByImportIdAsync(import.Id, ct);
        _importRepository.Remove(import);

        // Guardar cambios para que los filtros de borrado lógico apliquen en la consulta siguiente
        await _unitOfWork.SaveChangesAsync(ct);

        // 4. Reajustar CurrentBalance al saldo de la última transacción restante activa
        var latestTx = await _transactionRepository.GetLatestTransactionByAccountIdAsync(account.Id, ct);
        if (latestTx != null)
        {
            account.CurrentBalance = latestTx.BalanceAfter;
            account.BalanceLastUpdatedAt = latestTx.OperationDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        }
        else
        {
            account.CurrentBalance = null;
            account.BalanceLastUpdatedAt = null;
        }

        _accountRepository.Update(account);

        // 5. Persistir nuevo saldo
        await _unitOfWork.SaveChangesAsync(ct);

        return new RollbackStatementImportResult(true);
    }
}
