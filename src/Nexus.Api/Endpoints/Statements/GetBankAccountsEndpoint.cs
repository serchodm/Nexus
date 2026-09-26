using System.Security.Claims;
using FastEndpoints;
using Nexus.Application.Statements.Queries.GetBankAccounts;

namespace Nexus.Api.Endpoints.Statements;

public sealed class GetBankAccountsEndpoint : EndpointWithoutRequest<GetBankAccountsResult>
{
    private readonly GetBankAccountsQueryHandler _handler;

    public GetBankAccountsEndpoint(GetBankAccountsQueryHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Get("/accounts");
        Summary(s =>
        {
            s.Summary = "Obtiene el listado de cuentas bancarias del usuario";
            s.Description = "Devuelve todas las cuentas bancarias activas del usuario con su saldo y el cálculo consolidado del patrimonio neto ponderado.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(sub, out var userId))
        {
            ThrowError("Usuario no autenticado.", 401);
        }

        var result = await _handler.HandleAsync(new GetBankAccountsQuery(userId), ct);
        await Send.OkAsync(result, ct);
    }
}
