using System.Security.Claims;
using FastEndpoints;
using Nexus.Application.Transactions.Commands.SplitTransaction;
using Nexus.Application.Transactions.DTOs;
using Nexus.Domain.Common;

namespace Nexus.Api.Endpoints.Transactions;

public class SplitTransactionRequest
{
    public Guid Id { get; set; }
    public List<SplitItemInput> Items { get; set; } = new();
}

public sealed class SplitTransactionEndpoint : Endpoint<SplitTransactionRequest, TransactionDetailDto>
{
    private readonly SplitTransactionCommandHandler _handler;

    public SplitTransactionEndpoint(SplitTransactionCommandHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Post("/transactions/{id}/split");
        Summary(s =>
        {
            s.Summary = "Desglosa una transacción en múltiples partidas (Split Transactions)";
            s.Description = "Divide un movimiento mixto en 2 o más partidas con categorías e importes independientes, garantizando la invariante contable estricta.";
        });
    }

    public override async Task HandleAsync(SplitTransactionRequest req, CancellationToken ct)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(sub, out var userId))
        {
            ThrowError("Usuario no autenticado.", 401);
        }

        try
        {
            var cmd = new SplitTransactionCommand(
                TransactionId: req.Id,
                UserId: userId,
                Items: req.Items
            );

            var result = await _handler.HandleAsync(cmd, ct);
            await Send.OkAsync(result, ct);
        }
        catch (DomainValidationException ex)
        {
            ThrowError(ex.Message, 400);
        }
    }
}
