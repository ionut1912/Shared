using Microsoft.EntityFrameworkCore;
using Shared.Domain.Interfaces;

namespace Shared.Infra.Services;

/// <summary>
/// Provides a concrete implementation of the Unit of Work pattern
/// for Entity Framework Core.
/// </summary>
/// <remarks>
/// This class represents a transactional boundary, coordinating the
/// persistence of changes made through a single <see cref="DbContext"/>.
/// </remarks>
public class UnitOfWork : IUnitOfWork
{
    private readonly DbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitOfWork"/> class
    /// using the provided database context.
    /// </summary>
    /// <param name="context">
    /// The <see cref="DbContext"/> instance whose changes will be tracked
    /// and committed as part of this unit of work.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="context"/> is <c>null</c>.
    /// </exception>
    public UnitOfWork(DbContext context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        _context = context;
    }

    /// <summary>
    /// Persists all pending changes tracked by the underlying
    /// <see cref="DbContext"/> to the database.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the save operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous save operation.
    /// </returns>
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
