using Microsoft.EntityFrameworkCore.Storage;
using TheLightStore.Application.Interfaces;

namespace TheLightStore.Infrastructure.Persistence;

/// <summary>
/// Wrapper cho EF Core IDbContextTransaction để implement ITransaction abstraction
/// </summary>
public class EfTransaction : ITransaction
{
    private readonly IDbContextTransaction _transaction;

    public EfTransaction(IDbContextTransaction transaction)
    {
        _transaction = transaction;
    }

    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        return _transaction.CommitAsync(cancellationToken);
    }

    public Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        return _transaction.RollbackAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _transaction.DisposeAsync();
    }
}
