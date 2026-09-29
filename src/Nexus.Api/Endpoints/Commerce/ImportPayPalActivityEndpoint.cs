using System.Security.Claims;
using FastEndpoints;
using Nexus.Application.Commerce.Commands;

namespace Nexus.Api.Endpoints.Commerce;

public class ImportPayPalActivityRequest
{
    public IFormFile File { get; set; } = default!;
}

public sealed class ImportPayPalActivityEndpoint : Endpoint<ImportPayPalActivityRequest, ImportCommerceReportResult>
{
    private readonly ImportPayPalActivityCommandHandler _handler;

    public ImportPayPalActivityEndpoint(ImportPayPalActivityCommandHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Post("/commerce/paypal/import-activity");
        AllowFileUploads();
        Summary(s =>
        {
            s.Summary = "Importa un extracto de actividad oficial de PayPal (CSV)";
            s.Description = "Parsea la actividad de PayPal, identificando cargos y comercios finales, deduplicando por Transaction ID y reconciliando contra cargos bancarios.";
        });
    }

    public override async Task HandleAsync(ImportPayPalActivityRequest req, CancellationToken ct)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(sub, out var userId))
        {
            ThrowError("Usuario no autenticado.", 401);
        }

        if (req.File == null || req.File.Length == 0)
        {
            ThrowError("No se proporcionó ningún archivo o el archivo está vacío.", 400);
        }

        using var stream = req.File.OpenReadStream();
        var command = new ImportPayPalActivityCommand(userId, stream);
        var result = await _handler.HandleAsync(command, ct);

        await Send.OkAsync(result, ct);
    }
}
