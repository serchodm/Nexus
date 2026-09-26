using System.Security.Claims;
using FastEndpoints;
using Nexus.Application.Statements.Queries.GetAccountTransactions;

namespace Nexus.Api.Endpoints.Statements;

public class GetAccountTransactionsRequest
{
    public Guid Id { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
}

public sealed class GetAccountTransactionsEndpoint : Endpoint<GetAccountTransactionsRequest, GetAccountTransactionsResult>
{
    private readonly GetAccountTransactionsQueryHandler _handler;

    public GetAccountTransactionsEndpoint(GetAccountTransactionsQueryHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Get("/accounts/{id}/transactions");
        Summary(s =>
        {
            s.Summary = "Obtiene los movimientos bancarios de una cuenta";
            s.Description = "Devuelve el histórico cronológico paginado de movimientos con soporte de filtros por rango de fechas.";
        });
    }

    public override async Task HandleAsync(GetAccountTransactionsRequest req, CancellationToken ct)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(sub, out var userId))
        {
            ThrowError("Usuario no autenticado.", 401);
        }

        var query = new GetAccountTransactionsQuery(
            UserId: userId,
            AccountId: req.Id,
            Page: req.Page,
            PageSize: req.PageSize,
            FromDate: req.FromDate,
            ToDate: req.ToDate
        );

        var result = await _handler.HandleAsync(query, ct);

        if (result == null)
        {
            ThrowError("La cuenta especificada no existe o no tienes acceso a ella.", 404);
        }

        await Send.OkAsync(result, ct);
    }
}
