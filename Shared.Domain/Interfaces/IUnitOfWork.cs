using Microsoft.EntityFrameworkCore;

namespace Shared.Domain.Interfaces;

/// <summary>
/// Defines a Unit of Work abstraction for coordinating changes
/// across one or more repositories backed by an Entity Framework Core
/// <see cref="DbContext"/>.
/// </summary>
/// <remarks>
/// A unit of work represents a single transactional boundary,
/// ensuring that all changes are persisted atomically.
/// </remarks>
public interface IUnitOfWork
{
    /// <summary>
    /// Persists all pending changes made within the current unit of work
    /// to the underlying data store.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token used to cancel the save operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous save operation.
    /// </returns>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
