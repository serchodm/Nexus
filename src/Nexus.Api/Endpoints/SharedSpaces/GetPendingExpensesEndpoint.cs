using FastEndpoints;
using Nexus.Application.SharedExpenses.DTOs;
using Nexus.Application.SharedExpenses.Queries;

namespace Nexus.Api.Endpoints.SharedSpaces;

public class GetPendingExpensesRequest
{
    public Guid Id { get; set; }
}

public sealed class GetPendingExpensesEndpoint : Endpoint<GetPendingExpensesRequest, List<SharedExpenseDto>>
{
    private readonly GetPendingExpensesQueryHandler _handler;

    public GetPendingExpensesEndpoint(GetPendingExpensesQueryHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Get("/shared-spaces/{id}/pending");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Obtiene las partidas de gasto pendientes de liquidar";
            s.Description = "Devuelve la lista detallada de gastos que componen el balance deudor actual.";
        });
    }

    public override async Task HandleAsync(GetPendingExpensesRequest req, CancellationToken ct)
    {
        var result = await _handler.HandleAsync(new GetPendingExpensesQuery(req.Id), ct);
        await Send.OkAsync(result, ct);
    }
}
