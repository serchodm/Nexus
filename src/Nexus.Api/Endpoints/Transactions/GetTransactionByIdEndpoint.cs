using System.Security.Claims;
using FastEndpoints;
using Nexus.Application.Transactions.DTOs;
using Nexus.Application.Transactions.Queries.GetTransactionById;

namespace Nexus.Api.Endpoints.Transactions;

public class GetTransactionByIdRequest
{
    public Guid Id { get; set; }
}

public sealed class GetTransactionByIdEndpoint : Endpoint<GetTransactionByIdRequest, TransactionDetailDto>
{
    private readonly GetTransactionByIdQueryHandler _handler;

    public GetTransactionByIdEndpoint(GetTransactionByIdQueryHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Get("/transactions/{id}");
        Summary(s =>
        {
            s.Summary = "Obtiene el detalle completo de una transacción";
            s.Description = "Devuelve los datos enriquecidos de la transacción incluyendo partidas de desglose (splits) si existen.";
        });
    }

    public override async Task HandleAsync(GetTransactionByIdRequest req, CancellationToken ct)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(sub, out var userId))
        {
            ThrowError("Usuario no autenticado.", 401);
        }

        var result = await _handler.HandleAsync(new GetTransactionByIdQuery(req.Id, userId), ct);
        if (result == null)
        {
            ThrowError("La transacción no existe o no tienes acceso a ella.", 404);
        }

        await Send.OkAsync(result, ct);
    }
}
