using Nexus.Domain.Common;
using Nexus.Domain.Enums;
using Nexus.Domain.Repositories;

namespace Nexus.Application.Commerce.Commands;

public record RejectReconciliationResult(
    bool Success,
    Guid ReconciliationId,
    string? Message
);

public record RejectReconciliationCommand(
    Guid UserId,
    Guid ReconciliationId,
    string? Reason = null
);

public class RejectReconciliationCommandHandler
{
    private readonly ITransactionReconciliationRepository _reconciliationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RejectReconciliationCommandHandler(
        ITransactionReconciliationRepository reconciliationRepository,
        IUnitOfWork unitOfWork)
    {
        _reconciliationRepository = reconciliationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<RejectReconciliationResult> HandleAsync(RejectReconciliationCommand request, CancellationToken cancellationToken = default)
    {
        var rec = await _reconciliationRepository.GetByIdAsync(request.ReconciliationId, cancellationToken);
        if (rec == null || rec.UserId != request.UserId)
        {
            return new RejectReconciliationResult(false, Guid.Empty, "La propuesta de reconciliación no existe o no tienes acceso.");
        }

        if (rec.Status == ReconciliationStatus.Accepted)
        {
            return new RejectReconciliationResult(false, rec.Id, "No se puede descartar una reconciliación que ya fue aceptada y aplicada como partidas.");
        }

        rec.Status = ReconciliationStatus.Rejected;
        if (!string.IsNullOrWhiteSpace(request.Reason))
        {
            rec.MatchReason = $"{rec.MatchReason} [Descartada: {request.Reason}]";
        }

        _reconciliationRepository.Update(rec);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RejectReconciliationResult(true, rec.Id, "Propuesta de reconciliación descartada exitosamente.");
    }
}
