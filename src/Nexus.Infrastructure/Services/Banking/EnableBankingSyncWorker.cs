using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nexus.Application.Banking.Commands;
using Nexus.Domain.Repositories;

namespace Nexus.Infrastructure.Services.Banking;

public sealed class EnableBankingSyncWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EnableBankingSyncWorker> _logger;
    private readonly TimeSpan _syncInterval = TimeSpan.FromHours(6);

    public EnableBankingSyncWorker(
        IServiceProvider serviceProvider,
        ILogger<EnableBankingSyncWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EnableBankingSyncWorker iniciado con periodicidad de {Interval} horas.", _syncInterval.TotalHours);

        // Espera inicial de 30 segundos tras el arranque de la app para no saturar el inicio
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        using var timer = new PeriodicTimer(_syncInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunSyncCycleAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Error imprevisto durante el ciclo desatendido de sincronización Open Banking.");
            }

            if (!await timer.WaitForNextTickAsync(stoppingToken))
            {
                break;
            }
        }

        _logger.LogInformation("EnableBankingSyncWorker detenido.");
    }

    private async Task RunSyncCycleAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var connectionRepository = scope.ServiceProvider.GetRequiredService<IBankConnectionRepository>();
        var syncHandler = scope.ServiceProvider.GetRequiredService<SyncBankConnectionCommandHandler>();

        var activeConnections = await connectionRepository.GetActiveConnectionsAsync(ct);
        _logger.LogInformation("EnableBankingSyncWorker: procesando {Count} conexiones activas.", activeConnections.Count);

        foreach (var connection in activeConnections)
        {
            if (ct.IsCancellationRequested) break;

            try
            {
                var command = new SyncBankConnectionCommand(connection.UserId, connection.Id, ForceFullSync: false);
                var result = await syncHandler.HandleAsync(command, ct);

                if (result != null)
                {
                    _logger.LogInformation(
                        "Conexión {ConnectionId} ({Bank}) sincronizada con éxito: {NewTx} nuevas transacciones.",
                        connection.Id, connection.InstitutionName, result.NewTransactionsImported);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Fallo al sincronizar la conexión bancaria {ConnectionId} en segundo plano.", connection.Id);
            }
        }
    }
}
