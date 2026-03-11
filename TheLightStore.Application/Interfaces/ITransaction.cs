namespace TheLightStore.Application.Interfaces;

/// <summary>
/// Abstraction for database transaction - không phụ thuộc vào EF Core
/// </summary>
public interface ITransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
