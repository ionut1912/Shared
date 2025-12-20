using Microsoft.EntityFrameworkCore;
using Shared.Domain.Interfaces;

namespace Shared.Infra.Services
{
    /// <summary>
    /// Implements the unit of work pattern for a specific <see cref="DbContext"/> type.
    /// Provides a single point to commit changes to the database.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="DbContext"/> managed by this unit of work.</typeparam>
    public class UnitOfWork<T> : IUnitOfWork<T> where T : DbContext
    {
        private readonly T _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfWork{T}"/> class with the specified database context.
        /// </summary>
        /// <param name="context">The <see cref="DbContext"/> to manage.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public UnitOfWork(T context)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            _context = context;
        }

        /// <summary>
        /// Saves all changes made in the current unit of work to the database asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A token to observe while waiting for the save operation to complete.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous save operation.</returns>
        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
