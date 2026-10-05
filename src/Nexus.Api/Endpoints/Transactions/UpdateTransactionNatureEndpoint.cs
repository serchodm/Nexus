using System.Security.Claims;
using FastEndpoints;
using Nexus.Application.Transactions.Commands.UpdateTransactionNature;
using Nexus.Application.Transactions.DTOs;
using Nexus.Domain.Common;
using Nexus.Domain.Enums;

namespace Nexus.Api.Endpoints.Transactions;

public class UpdateTransactionNatureRequest
{
    public Guid Id { get; set; }
    public TransactionNature Nature { get; set; }
}

public sealed class UpdateTransactionNatureEndpoint : Endpoint<UpdateTransactionNatureRequest, TransactionDetailDto>
{
    private readonly UpdateTransactionNatureCommandHandler _handler;

    public UpdateTransactionNatureEndpoint(UpdateTransactionNatureCommandHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Patch("/transactions/{id}/nature");
        Summary(s =>
        {
            s.Summary = "Actualiza la naturaleza contable de una transacción";
            s.Description = "Permite reclasificar un movimiento como gasto estándar, contra-partida (reembolso), ingreso real o traspaso interno.";
        });
    }

    public override async Task HandleAsync(UpdateTransactionNatureRequest req, CancellationToken ct)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(sub, out var userId))
        {
            ThrowError("Usuario no autenticado.", 401);
        }

        try
        {
            var cmd = new UpdateTransactionNatureCommand(req.Id, req.Nature, userId);
            var result = await _handler.HandleAsync(cmd, ct);
            await Send.OkAsync(result, ct);
        }
        catch (DomainValidationException ex)
        {
            ThrowError(ex.Message, 400);
        }
    }
}
