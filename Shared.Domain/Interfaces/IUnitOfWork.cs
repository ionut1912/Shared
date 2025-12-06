using Microsoft.EntityFrameworkCore;

namespace Shared.Domain.Interfaces;

public interface IUnitOfWork<T> where T : DbContext
{    
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    
}