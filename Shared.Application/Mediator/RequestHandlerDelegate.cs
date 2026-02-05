namespace Shared.Application.Mediator;

/// <summary>
/// Represents a handler delegate for the next step in the pipeline
/// </summary>
/// <typeparam name="TResponse">The response type</typeparam>
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();

/// <summary>
/// Pipeline behavior to surround the inner handler
/// Implementations add additional behavior and await the next delegate
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public interface IPipelineBehavior<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Pipeline handler. Perform any additional behavior and await the next delegate as necessary
    /// </summary>
    /// <param name="request">Incoming request</param>
    /// <param name="next">Awaitable delegate for the next action in the pipeline</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Awaitable task returning the response</returns>
    Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
}