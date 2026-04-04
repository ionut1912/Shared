using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Common;
using Shared.Domain.Interfaces;

namespace Shared.Infra.Services;

/// <inheritdoc />
public class GenericRepository<T> : IGenericRepository<T> where T : Entity
{
    private readonly DbSet<T> _dbSet;

    /// <inheritdoc />
    public GenericRepository(DbSet<T> dbset)
    {
        ArgumentNullException.ThrowIfNull(dbset);
        _dbSet = dbset;
    }

    /// <inheritdoc />
    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<T>> GetAllAsync(
        CancellationToken cancellationToken = default,
        params Expression<Func<T, object>>[] includes)
    {
        includes ??= Array.Empty<Expression<Func<T, object>>>();

        var query = _dbSet.AsNoTracking();

        foreach (var include in includes) query = query.Include(include);

        return await query.ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<T?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default,
        params Expression<Func<T, object>>[] includes)
    {
        includes ??= Array.Empty<Expression<Func<T, object>>>();

        var query = _dbSet.AsNoTracking();

        foreach (var include in includes) query = query.Include(include);

        return await query.FirstOrDefaultAsync(
                e => EF.Property<Guid>(e, "Id") == id,
                cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }
}