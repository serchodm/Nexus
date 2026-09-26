using System.Security.Claims;
using FastEndpoints;
using Nexus.Application.Transactions.Commands.CategorizeTransaction;
using Nexus.Application.Transactions.DTOs;
using Nexus.Domain.Common;
using Nexus.Domain.Enums;

namespace Nexus.Api.Endpoints.Transactions;

public class CategorizeTransactionRequest
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public Guid? SubcategoryId { get; set; }
    public ExpenseNature? ExpenseNature { get; set; }
    public ExpenseFlexibility? ExpenseFlexibility { get; set; }
    public bool? IsRecurring { get; set; }
    public bool CreateRuleForMerchant { get; set; } = false;
}

public sealed class CategorizeTransactionEndpoint : Endpoint<CategorizeTransactionRequest, TransactionDetailDto>
{
    private readonly CategorizeTransactionCommandHandler _handler;

    public CategorizeTransactionEndpoint(CategorizeTransactionCommandHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Patch("/transactions/{id}/categorize");
        Summary(s =>
        {
            s.Summary = "Categoriza manualmente una transacción";
            s.Description = "Permite asignar una categoría y subcategoría a un movimiento bancario, con la opción de generar una regla automática y aplicarla de forma retroactiva.";
        });
    }

    public override async Task HandleAsync(CategorizeTransactionRequest req, CancellationToken ct)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(sub, out var userId))
        {
            ThrowError("Usuario no autenticado.", 401);
        }

        try
        {
            var cmd = new CategorizeTransactionCommand(
                TransactionId: req.Id,
                UserId: userId,
                CategoryId: req.CategoryId,
                SubcategoryId: req.SubcategoryId,
                ExpenseNature: req.ExpenseNature,
                ExpenseFlexibility: req.ExpenseFlexibility,
                IsRecurring: req.IsRecurring,
                CreateRuleForMerchant: req.CreateRuleForMerchant
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
