using FastEndpoints;

namespace Nexus.Api.Endpoints.Health;

public sealed class HealthCheckResponse
{
    public string Status { get; set; } = "Healthy";
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public string Version { get; set; } = "1.0.0";
}

public sealed class HealthCheckEndpoint : EndpointWithoutRequest<HealthCheckResponse>
{
    public override void Configure()
    {
        Get("/health");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Comprueba el estado de salud de la API";
            s.Description = "Devuelve el estado operativo de Nexus API en formato UTC";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await Send.OkAsync(new HealthCheckResponse(), ct);
    }
}
