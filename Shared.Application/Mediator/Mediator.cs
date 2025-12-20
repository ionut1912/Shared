using Microsoft.Extensions.DependencyInjection;

namespace Shared.Application.Mediator;

/// <summary>
/// Default mediator implementation that resolves handlers and pipeline behaviors
/// from the dependency injection container.
/// </summary>
public sealed class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="Mediator"/> class.
    /// </summary>
    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <inheritdoc />
    public async Task<TResponse> Send<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<,>)
            .MakeGenericType(requestType, typeof(TResponse));

        var handler = _serviceProvider.GetService(handlerType)
            ?? throw new InvalidOperationException(
                $"No handler registered for {requestType.Name}");

        var behaviorType = typeof(IPipelineBehavior<,>)
            .MakeGenericType(requestType, typeof(TResponse));

        var behaviors = _serviceProvider
            .GetServices(behaviorType)
            .Reverse()
            .ToList();

        var handleMethod = handlerType.GetMethod("Handle")
            ?? throw new InvalidOperationException(
                $"Handle method not found for {requestType.Name}");

        Task<TResponse> Handler() =>
            InvokeHandler<TResponse>(handleMethod, handler, request, cancellationToken);

        var pipeline = behaviors.Aggregate(
            (RequestHandlerDelegate<TResponse>)Handler,
            (next, behavior) =>
            {
                var behaviorMethod = behaviorType.GetMethod("Handle")
                    ?? throw new InvalidOperationException(
                        "Handle method not found on behavior");

                return () =>
                {
                    var result = behaviorMethod.Invoke(
                        behavior,
                        new object[] { request, next, cancellationToken });

                    if (result is Task<TResponse> task)
                        return task;

                    throw new InvalidOperationException(
                        "Behavior did not return expected Task<TResponse>");
                };
            });

        return await pipeline().ConfigureAwait(false);
    }

    private static async Task<TResponse> InvokeHandler<TResponse>(
        System.Reflection.MethodInfo handleMethod,
        object handler,
        object request,
        CancellationToken cancellationToken)
    {
        var result = handleMethod.Invoke(
            handler,
            new object[] { request, cancellationToken });

        if (result is Task<TResponse> task)
            return await task.ConfigureAwait(false);

        throw new InvalidOperationException(
            $"Handler did not return expected Task<{typeof(TResponse).Name}>");
    }
}
