using FastEndpoints;
using Nexus.Application.SharedExpenses.Commands;
using Nexus.Application.SharedExpenses.DTOs;
using Nexus.Domain.Enums;

namespace Nexus.Api.Endpoints.SharedSpaces;

public class SettleBalanceRequest
{
    public Guid Id { get; set; }
    public Guid PayerMemberId { get; set; }
    public Guid ReceiverMemberId { get; set; }
    public decimal Amount { get; set; }
    public SettlementMethod Method { get; set; } = SettlementMethod.Bizum;
    public string? ReferenceCode { get; set; }
}

public sealed class SettleBalanceEndpoint : Endpoint<SettleBalanceRequest, SharedSettlementDto>
{
    private readonly SettleBalanceCommandHandler _handler;

    public SettleBalanceEndpoint(SettleBalanceCommandHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Post("/shared-spaces/{id}/settle");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Liquida y extingue el saldo deudor del espacio compartido";
            s.Description = "Marca las partidas pendientes como saldadas y registra la liquidación vía Bizum, SEPA u otra modalidad.";
        });
    }

    public override async Task HandleAsync(SettleBalanceRequest req, CancellationToken ct)
    {
        var command = new SettleBalanceCommand(
            req.Id,
            req.PayerMemberId,
            req.ReceiverMemberId,
            req.Amount,
            req.Method,
            req.ReferenceCode
        );

        var result = await _handler.HandleAsync(command, ct);
        if (result == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(result, ct);
    }
}
