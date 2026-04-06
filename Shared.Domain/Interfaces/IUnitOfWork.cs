/// <summary>
///     Defines a unit of work that wraps database operations in a single transaction.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    ///     Persists all pending changes to the database.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    /// <summary>
    ///     Begins a new database transaction.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    Task BeginTransactionAsync(CancellationToken ct = default);

    /// <summary>
    ///     Commits the current transaction and persists all changes atomically.
    ///     Does nothing if no transaction is active.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    Task CommitAsync(CancellationToken ct = default);

    /// <summary>
    ///     Rolls back the current transaction and discards all pending changes.
    ///     Does nothing if no transaction is active.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    Task RollbackAsync(CancellationToken ct = default);
}