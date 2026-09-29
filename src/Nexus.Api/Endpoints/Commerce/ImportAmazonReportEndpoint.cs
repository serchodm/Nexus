using System.Security.Claims;
using FastEndpoints;
using Nexus.Application.Commerce.Commands;

namespace Nexus.Api.Endpoints.Commerce;

public class ImportAmazonReportRequest
{
    public IFormFile File { get; set; } = default!;
}

public sealed class ImportAmazonReportEndpoint : Endpoint<ImportAmazonReportRequest, ImportCommerceReportResult>
{
    private readonly ImportAmazonOrderReportCommandHandler _handler;

    public ImportAmazonReportEndpoint(ImportAmazonOrderReportCommandHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Post("/commerce/amazon/import-report");
        AllowFileUploads();
        Summary(s =>
        {
            s.Summary = "Importa un reporte de pedidos oficial de Amazon (CSV)";
            s.Description = "Parsea el fichero CSV oficial de pedidos de Amazon, deduplica pedidos existentes y dispara automáticamente el motor de reconciliación.";
        });
    }

    public override async Task HandleAsync(ImportAmazonReportRequest req, CancellationToken ct)
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
        var command = new ImportAmazonOrderReportCommand(userId, stream);
        var result = await _handler.HandleAsync(command, ct);

        await Send.OkAsync(result, ct);
    }
}
