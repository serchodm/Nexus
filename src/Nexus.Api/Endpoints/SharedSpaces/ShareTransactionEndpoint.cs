using FastEndpoints;
using Nexus.Application.SharedExpenses.Commands;
using Nexus.Application.SharedExpenses.DTOs;

namespace Nexus.Api.Endpoints.SharedSpaces;

public class ShareTransactionRequest
{
    public Guid Id { get; set; } // SharedSpaceId from route
    public Guid BankTransactionId { get; set; }
    public Guid? TransactionSplitItemId { get; set; }
    public Guid PaidByMemberId { get; set; }
    public decimal PayerPercentage { get; set; } = 55.0m;
    public string? CustomDescription { get; set; }
}

public sealed class ShareTransactionEndpoint : Endpoint<ShareTransactionRequest, SharedExpenseDto>
{
    private readonly ShareTransactionCommandHandler _handler;

    public ShareTransactionEndpoint(ShareTransactionCommandHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Post("/shared-spaces/{id}/share");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Imputa una transacción bancaria o partida al espacio compartido";
            s.Description = "Calcula las cuotas según el ratio acordado y actualiza el saldo deudor de la contraparte.";
        });
    }

    public override async Task HandleAsync(ShareTransactionRequest req, CancellationToken ct)
    {
        var command = new ShareTransactionCommand(
            req.Id,
            req.BankTransactionId,
            req.TransactionSplitItemId,
            req.PaidByMemberId,
            req.PayerPercentage,
            req.CustomDescription
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
