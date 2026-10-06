using FastEndpoints;
using Nexus.Application.SharedExpenses.Commands;
using Nexus.Domain.Enums;

namespace Nexus.Api.Endpoints.SharedSpaces;

public class UpdateSplitRatioRequest
{
    public Guid Id { get; set; }
    public SplitRuleType RuleType { get; set; } = SplitRuleType.FixedRatio;
    public decimal OwnerPercentage { get; set; } = 55.0m;
    public decimal? OwnerIncome { get; set; }
    public decimal? MemberIncome { get; set; }
}

public class UpdateSplitRatioResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public sealed class UpdateSplitRatioEndpoint : Endpoint<UpdateSplitRatioRequest, UpdateSplitRatioResponse>
{
    private readonly UpdateSplitConfigurationCommandHandler _handler;

    public UpdateSplitRatioEndpoint(UpdateSplitConfigurationCommandHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Patch("/shared-spaces/{id}/ratios");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Actualiza las reglas de reparto del espacio compartido";
            s.Description = "Permite modificar el ratio fijo manual o recalcular el porcentaje proporcional a los ingresos (Fair-Share).";
        });
    }

    public override async Task HandleAsync(UpdateSplitRatioRequest req, CancellationToken ct)
    {
        var command = new UpdateSplitConfigurationCommand(
            req.Id,
            req.RuleType,
            req.OwnerPercentage,
            req.OwnerIncome,
            req.MemberIncome
        );

        var success = await _handler.HandleAsync(command, ct);
        if (!success)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(new UpdateSplitRatioResponse
        {
            Success = true,
            Message = "Configuración de reparto actualizada correctamente."
        }, ct);
    }
}
