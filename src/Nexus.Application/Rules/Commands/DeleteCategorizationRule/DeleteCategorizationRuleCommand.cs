using Nexus.Domain.Common;
using Nexus.Domain.Repositories;

namespace Nexus.Application.Rules.Commands.DeleteCategorizationRule;

public record DeleteCategorizationRuleCommand(Guid Id, Guid UserId);

public class DeleteCategorizationRuleCommandHandler
{
    private readonly ICategorizationRuleRepository _ruleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCategorizationRuleCommandHandler(
        ICategorizationRuleRepository ruleRepository,
        IUnitOfWork unitOfWork)
    {
        _ruleRepository = ruleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(DeleteCategorizationRuleCommand cmd, CancellationToken ct = default)
    {
        var rule = await _ruleRepository.GetByIdAsync(cmd.Id, ct);
        if (rule == null || rule.UserId != cmd.UserId)
        {
            return;
        }

        _ruleRepository.Delete(rule);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
