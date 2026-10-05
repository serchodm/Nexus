using System.Security.Claims;
using FastEndpoints;
using Nexus.Application.Transactions.Commands.LinkTransaction;
using Nexus.Application.Transactions.DTOs;
using Nexus.Domain.Common;

namespace Nexus.Api.Endpoints.Transactions;

public class LinkTransactionRequest
{
    public Guid Id { get; set; }
    public Guid TargetTransactionId { get; set; }
}

public sealed class LinkTransactionEndpoint : Endpoint<LinkTransactionRequest, TransactionDetailDto>
{
    private readonly LinkTransactionCommandHandler _handler;

    public LinkTransactionEndpoint(LinkTransactionCommandHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Post("/transactions/{id}/link");
        Summary(s =>
        {
            s.Summary = "Vincula una transacción a otra (devolución o traspaso)";
            s.Description = "Permite enlazar un abono de devolución a su cargo original o emparejar dos extremos de un traspaso propio entre cuentas.";
        });
    }

    public override async Task HandleAsync(LinkTransactionRequest req, CancellationToken ct)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(sub, out var userId))
        {
            ThrowError("Usuario no autenticado.", 401);
        }

        try
        {
            var cmd = new LinkTransactionCommand(req.Id, req.TargetTransactionId, userId);
            var result = await _handler.HandleAsync(cmd, ct);
            await Send.OkAsync(result, ct);
        }
        catch (DomainValidationException ex)
        {
            ThrowError(ex.Message, 400);
        }
    }
}
