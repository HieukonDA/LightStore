namespace TheLightStore.Interfaces.Inventory;

public interface IInventoryCleanupJob
{
    Task RunAsync(CancellationToken ct = default);
}
