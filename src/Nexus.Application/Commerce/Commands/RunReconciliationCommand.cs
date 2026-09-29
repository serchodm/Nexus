using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Nexus.Domain.Enums;
using Nexus.Domain.Repositories;
using Nexus.Domain.Services;

namespace Nexus.Application.Commerce.Commands;

public record RunReconciliationResult(
    int EvaluatedTransactions,
    int MatchesFound,
    int NewCandidatesCreated
);

public record RunReconciliationCommand(
    Guid UserId,
    Guid? BankAccountId = null
);

public class RunReconciliationCommandHandler
{
    private readonly IBankTransactionRepository _transactionRepository;
    private readonly IBankAccountRepository _accountRepository;
    private readonly IExternalCommerceOrderRepository _orderRepository;
    private readonly ITransactionReconciliationRepository _reconciliationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CommerceReconciliationEngine _reconciliationEngine = new();

    public RunReconciliationCommandHandler(
        IBankTransactionRepository transactionRepository,
        IBankAccountRepository accountRepository,
        IExternalCommerceOrderRepository orderRepository,
        ITransactionReconciliationRepository reconciliationRepository,
        IUnitOfWork unitOfWork)
    {
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
        _orderRepository = orderRepository;
        _reconciliationRepository = reconciliationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<RunReconciliationResult> HandleAsync(RunReconciliationCommand request, CancellationToken cancellationToken = default)
    {
        var userAccounts = await _accountRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (request.BankAccountId.HasValue)
        {
            userAccounts = userAccounts.Where(a => a.Id == request.BankAccountId.Value).ToList();
        }

        if (userAccounts.Count == 0)
        {
            return new RunReconciliationResult(0, 0, 0);
        }

        var candidateOrders = await _orderRepository.GetUnreconciledByUserIdAsync(request.UserId, cancellationToken);
        if (candidateOrders.Count == 0)
        {
            return new RunReconciliationResult(0, 0, 0);
        }

        var evaluated = 0;
        var matches = 0;
        var newCandidates = 0;

        foreach (var account in userAccounts)
        {
            // Consultar transacciones de gastos (Amount < 0)
            var (transactions, _) = await _transactionRepository.GetByAccountIdAsync(
                account.Id,
                pageSize: 200,
                ct: cancellationToken);

            foreach (var tx in transactions)
            {
                if (tx.Amount >= 0) continue;
                evaluated++;

                // Comprobar si ya tiene una reconciliación aceptada o existente
                var existingReconciliation = await _reconciliationRepository.GetByTransactionIdAsync(tx.Id, cancellationToken);
                if (existingReconciliation != null) continue;

                var candidate = _reconciliationEngine.FindBestMatch(tx, candidateOrders);
                if (candidate != null)
                {
                    matches++;
                    var rec = new TransactionReconciliation
                    {
                        UserId = request.UserId,
                        BankTransactionId = candidate.BankTransactionId,
                        ExternalCommerceOrderId = candidate.ExternalCommerceOrderId,
                        Status = ReconciliationStatus.Candidate,
                        Confidence = candidate.Confidence,
                        MatchReason = candidate.MatchReason,
                        MatchedOrderItemIds = candidate.MatchedOrderItemIds,
                        ReconciledAt = DateTime.UtcNow
                    };

                    await _reconciliationRepository.AddAsync(rec, cancellationToken);
                    newCandidates++;
                }
            }
        }

        if (newCandidates > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return new RunReconciliationResult(evaluated, matches, newCandidates);
    }
}
