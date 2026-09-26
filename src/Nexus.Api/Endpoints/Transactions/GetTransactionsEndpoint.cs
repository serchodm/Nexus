using System.Security.Claims;
using FastEndpoints;
using Nexus.Application.Transactions.DTOs;
using Nexus.Application.Transactions.Queries.GetTransactions;
using Nexus.Domain.Enums;

namespace Nexus.Api.Endpoints.Transactions;

public class GetTransactionsRequest
{
    public Guid? BankAccountId { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? SubcategoryId { get; set; }
    public string? SearchTerm { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public bool? IsRecurring { get; set; }
    public bool? HasSplits { get; set; }
    public ExpenseNature? ExpenseNature { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public sealed class GetTransactionsEndpoint : Endpoint<GetTransactionsRequest, TransactionsPagedResult>
{
    private readonly GetTransactionsQueryHandler _handler;

    public GetTransactionsEndpoint(GetTransactionsQueryHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Get("/transactions");
        Summary(s =>
        {
            s.Summary = "Búsqueda paginada y filtrado de transacciones";
            s.Description = "Permite filtrar movimientos por cuenta, categoría, subcategoría, comercio limpio, recurrencia, fechas, naturaleza dual o presencia de desgloses.";
        });
    }

    public override async Task HandleAsync(GetTransactionsRequest req, CancellationToken ct)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(sub, out var userId))
        {
            ThrowError("Usuario no autenticado.", 401);
        }

        var query = new GetTransactionsQuery(
            UserId: userId,
            BankAccountId: req.BankAccountId,
            CategoryId: req.CategoryId,
            SubcategoryId: req.SubcategoryId,
            SearchTerm: req.SearchTerm,
            FromDate: req.FromDate,
            ToDate: req.ToDate,
            IsRecurring: req.IsRecurring,
            HasSplits: req.HasSplits,
            ExpenseNature: req.ExpenseNature,
            Page: req.Page,
            PageSize: req.PageSize
        );

        var result = await _handler.HandleAsync(query, ct);
        await Send.OkAsync(result, ct);
    }
}
