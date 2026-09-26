using System.Security.Claims;
using FastEndpoints;
using Nexus.Application.Transactions.Commands.ResetTransactionSplits;
using Nexus.Application.Transactions.DTOs;
using Nexus.Domain.Common;

namespace Nexus.Api.Endpoints.Transactions;

public class ResetTransactionSplitsRequest
{
    public Guid Id { get; set; }
}

public sealed class ResetTransactionSplitsEndpoint : Endpoint<ResetTransactionSplitsRequest, TransactionDetailDto>
{
    private readonly ResetTransactionSplitsCommandHandler _handler;

    public ResetTransactionSplitsEndpoint(ResetTransactionSplitsCommandHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Delete("/transactions/{id}/split");
        Summary(s =>
        {
            s.Summary = "Restaura una transacción desglosada eliminando sus partidas";
            s.Description = "Elimina todas las partidas de desglose y restaura la imputación íntegra de la transacción a su categoría principal.";
        });
    }

    public override async Task HandleAsync(ResetTransactionSplitsRequest req, CancellationToken ct)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(sub, out var userId))
        {
            ThrowError("Usuario no autenticado.", 401);
        }

        try
        {
            var result = await _handler.HandleAsync(new ResetTransactionSplitsCommand(req.Id, userId), ct);
            await Send.OkAsync(result, ct);
        }
        catch (DomainValidationException ex)
        {
            ThrowError(ex.Message, 400);
        }
    }
}
