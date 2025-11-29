using Freelance.Shared.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Shared.Infra.Services;

public class UnitOfWork<T> : IUnitOfWork<T> where T : DbContext
{
    private readonly T _context;

    public UnitOfWork(T context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        _context = context;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}