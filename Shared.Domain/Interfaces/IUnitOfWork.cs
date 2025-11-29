namespace Shared.Domain.Interfaces;

public interface IUnitOfWork<T>
{    
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    
}