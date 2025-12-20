using Microsoft.EntityFrameworkCore;

namespace Shared.Domain.Interfaces
{
    /// <summary>
    /// Represents a unit of work pattern for a database context.
    /// Encapsulates transaction management and ensures that changes are saved atomically.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="DbContext"/> used by this unit of work.</typeparam>
    public interface IUnitOfWork<T> where T : DbContext
    {
        /// <summary>
        /// Saves all changes made in the current unit of work to the database asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A token to observe while waiting for the operation to complete.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous save operation.</returns>
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
