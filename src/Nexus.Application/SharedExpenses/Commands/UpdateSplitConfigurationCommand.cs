using Nexus.Domain.Common;
using Nexus.Domain.Enums;
using Nexus.Domain.Repositories;
using Nexus.Domain.Services;

namespace Nexus.Application.SharedExpenses.Commands;

public record UpdateSplitConfigurationCommand(
    Guid SharedSpaceId,
    SplitRuleType RuleType,
    decimal OwnerPercentage,
    decimal? OwnerIncome,
    decimal? MemberIncome
);

public class UpdateSplitConfigurationCommandHandler
{
    private readonly ISharedSpaceRepository _spaceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSplitConfigurationCommandHandler(
        ISharedSpaceRepository spaceRepository,
        IUnitOfWork unitOfWork)
    {
        _spaceRepository = spaceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> HandleAsync(UpdateSplitConfigurationCommand command, CancellationToken ct = default)
    {
        var space = await _spaceRepository.GetByIdWithMembersAsync(command.SharedSpaceId, ct);
        if (space == null)
        {
            return false;
        }

        space.DefaultSplitRule = command.RuleType;

        if (command.RuleType == SplitRuleType.ProportionalToIncome && command.OwnerIncome.HasValue && command.MemberIncome.HasValue)
        {
            var (ownerPct, memberPct) = FairShareCalculator.CalculateIncomeProportions(command.OwnerIncome.Value, command.MemberIncome.Value);
            space.DefaultOwnerPercentage = ownerPct;
            space.DefaultMemberPercentage = memberPct;
        }
        else
        {
            decimal ownerPct = Math.Clamp(command.OwnerPercentage, 0m, 100m);
            space.DefaultOwnerPercentage = ownerPct;
            space.DefaultMemberPercentage = 100m - ownerPct;
        }

        _spaceRepository.Update(space);
        await _unitOfWork.SaveChangesAsync(ct);
        return true;
    }
}
