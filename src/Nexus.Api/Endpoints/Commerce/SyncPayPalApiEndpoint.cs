using System.Security.Claims;
using FastEndpoints;
using Nexus.Application.Commerce.Commands;

namespace Nexus.Api.Endpoints.Commerce;

public class SyncPayPalApiRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public sealed class SyncPayPalApiEndpoint : Endpoint<SyncPayPalApiRequest, ImportCommerceReportResult>
{
    private readonly SyncPayPalApiCommandHandler _handler;

    public SyncPayPalApiEndpoint(SyncPayPalApiCommandHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Post("/commerce/paypal/sync");
        Summary(s =>
        {
            s.Summary = "Sincroniza transacciones directamente desde la API de PayPal";
            s.Description = "Descarga la actividad reciente de la cuenta conectada de PayPal, deduplica movimientos y ejecuta el motor de reconciliación.";
        });
    }

    public override async Task HandleAsync(SyncPayPalApiRequest req, CancellationToken ct)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(sub, out var userId))
        {
            ThrowError("Usuario no autenticado.", 401);
        }

        try
        {
            var command = new SyncPayPalApiCommand(userId, req.StartDate, req.EndDate);
            var result = await _handler.HandleAsync(command, ct);
            await Send.OkAsync(result, ct);
        }
        catch (InvalidOperationException ex)
        {
            ThrowError(ex.Message, 400);
        }
        catch (Exception ex)
        {
            ThrowError($"Error de sincronización con PayPal: {ex.Message}", 502);
        }
    }
}
