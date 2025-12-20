using Microsoft.EntityFrameworkCore;
using Shared.Application.Interfaces;
using System.Linq.Expressions;

namespace Shared.Infra.Services
{
    /// <inheritdoc />
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly DbSet<T> _dbSet;

        /// <inheritdoc />
        public GenericRepository(DbSet<T> dbset)
        {
            ArgumentNullException.ThrowIfNull(dbset, nameof(dbset));
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

            IQueryable<T> query = _dbSet.AsNoTracking();

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

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

            IQueryable<T> query = _dbSet.AsNoTracking();

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

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
}
