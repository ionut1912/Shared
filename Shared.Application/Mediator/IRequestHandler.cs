namespace Shared.Application.Mediator;

/// <summary>
/// Defines a handler for a request that returns a response.
/// </summary>
/// <typeparam name="TRequest">
/// The type of request being handled.
/// </typeparam>
/// <typeparam name="TResponse">
/// The type of response returned by the handler.
/// </typeparam>
public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handles the specified request asynchronously.
    /// </summary>
    /// <param name="request">
    /// The request instance to handle.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to observe while waiting for the operation to complete.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the response produced by the handler.
    /// </returns>
    Task<TResponse> Handle(
        TRequest request,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines a handler for a request that does not return a response.
/// </summary>
/// <typeparam name="TRequest">
/// The type of request being handled.
/// </typeparam>
public interface IRequestHandler<in TRequest> : IRequestHandler<TRequest, Unit>
    where TRequest : IRequest<Unit>
{
}
