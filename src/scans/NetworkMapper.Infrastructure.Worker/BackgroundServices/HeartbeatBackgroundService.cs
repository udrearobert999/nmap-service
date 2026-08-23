using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetworkMapper.Contracts.Constants;
using NetworkMapper.Domain.Abstractions;

namespace NetworkMapper.Infrastructure.Worker.BackgroundServices;

internal sealed class HeartbeatBackgroundService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(10);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<HeartbeatBackgroundService> _logger;

    public HeartbeatBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<HeartbeatBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);

        do
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var store = scope.ServiceProvider.GetRequiredService<IServiceHeartbeatStore>();
                await store.UpsertAsync(ServiceNames.Scanning, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write scanning heartbeat.");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
