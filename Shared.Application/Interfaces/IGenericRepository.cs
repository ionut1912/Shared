using System.Linq.Expressions;

namespace Shared.Application.Interfaces;

/// <summary>
/// Defines a generic repository for basic CRUD operations on entities.
/// </summary>
/// <typeparam name="T">
/// The entity type managed by the repository.
/// </typeparam>
public interface IGenericRepository<T> where T : class
{
    /// <summary>
    /// Asynchronously adds a new entity to the repository.
    /// </summary>
    /// <param name="entity">
    /// The entity to add.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to observe while waiting for the task to complete.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous add operation.
    /// </returns>
    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves all entities of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token to observe while waiting for the task to complete.
    /// </param>
    /// <param name="includes">
    /// Navigation property expressions to include in the query.
    /// If no includes are specified, only the root entities are returned.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains all matching entities.
    /// </returns>
    Task<IEnumerable<T>> GetAllAsync(
        CancellationToken cancellationToken = default,
        params Expression<Func<T, object>>[] includes);

    /// <summary>
    /// Asynchronously retrieves an entity by its unique identifier.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the entity.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to observe while waiting for the task to complete.
    /// </param>
    /// <param name="includes">
    /// Navigation property expressions to include in the query.
    /// If no includes are specified, only the root entity is returned.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the entity if found; otherwise, <c>null</c>.
    /// </returns>
    Task<T?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default,
        params Expression<Func<T, object>>[] includes);

    /// <summary>
    /// Marks an existing entity as modified in the repository.
    /// </summary>
    /// <param name="entity">
    /// The entity to update.
    /// </param>
    void Update(T entity);

    /// <summary>
    /// Marks an entity for deletion from the repository.
    /// </summary>
    /// <param name="entity">
    /// The entity to delete.
    /// </param>
    void Delete(T entity);
}
