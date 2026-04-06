using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     EF Core implementation of <see cref="IUnitOfWork" />.
///     Wraps <see cref="DbContext" /> to manage transactions and change persistence.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly DbContext _db;
    private IDbContextTransaction? _transaction;

    /// <summary>
    ///     Initializes a new instance of <see cref="UnitOfWork" />.
    /// </summary>
    /// <param name="db">The database context instance.</param>
    public UnitOfWork(DbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);

    /// <inheritdoc />
    public async Task BeginTransactionAsync(CancellationToken ct = default)
        => _transaction = await _db.Database.BeginTransactionAsync(ct);

    /// <inheritdoc />
    public async Task CommitAsync(CancellationToken ct = default)
    {
        if (_transaction is null) return;
        await _transaction.CommitAsync(ct);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    /// <inheritdoc />
    public async Task RollbackAsync(CancellationToken ct = default)
    {
        if (_transaction is null) return;
        await _transaction.RollbackAsync(ct);
        await _transaction.DisposeAsync();
        _transaction = null;
    }
}