using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TheLightStore.Interfaces.Inventory;

namespace TheLightStore.Services.BackgroundJobs;

public class InventoryCleanupJob : BackgroundService, IInventoryCleanupJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<InventoryCleanupJob> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(5); // có thể config từ appsettings

    public InventoryCleanupJob(
        IServiceScopeFactory scopeFactory,
        ILogger<InventoryCleanupJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    // Background loop
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var inventoryService = scope.ServiceProvider.GetRequiredService<IInventoryService>();

            try
            {
                await inventoryService.CleanupExpiredReservationsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up reservations");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    // Cho phép service khác gọi trực tiếp (manual trigger)
    public async Task RunAsync(CancellationToken ct = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var inventoryService = scope.ServiceProvider.GetRequiredService<IInventoryService>();

        try
        {
            await inventoryService.CleanupExpiredReservationsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while cleaning up inventory reservations");
        }
    }
}
