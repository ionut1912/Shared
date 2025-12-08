using Microsoft.Extensions.DependencyInjection;

namespace Shared.Application.Mediator;

/// <summary>
/// Default mediator implementation that resolves handlers from the DI container
/// </summary>
public sealed class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;

    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));

        var handler = _serviceProvider.GetService(handlerType)
            ?? throw new InvalidOperationException($"No handler registered for {requestType.Name}");

        // Get pipeline behaviors
        var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResponse));
        var behaviors = _serviceProvider.GetServices(behaviorType).Reverse().ToList();

        // Build the pipeline
        var handleMethod = handlerType.GetMethod("Handle")
            ?? throw new InvalidOperationException($"Handle method not found on handler for {requestType.Name}");

        Task<TResponse> Handler() => InvokeHandler<TResponse>(handleMethod, handler, request, cancellationToken);

        var pipeline = behaviors.Aggregate(
            (RequestHandlerDelegate<TResponse>)Handler,
            (next, behavior) =>
            {
                var behaviorMethod = behaviorType.GetMethod("Handle")
                    ?? throw new InvalidOperationException("Handle method not found on behavior");

                return () =>
                {
                    var result = behaviorMethod.Invoke(behavior, [request, next, cancellationToken]);
                    return result is Task<TResponse> taskResult
                        ? taskResult
                        : throw new InvalidOperationException("Behavior did not return expected Task<TResponse>");
                };
            });

        return await pipeline();
    }

    private static async Task<TResponse> InvokeHandler<TResponse>(
        System.Reflection.MethodInfo handleMethod,
        object handler,
        object request,
        CancellationToken cancellationToken)
    {
        var result = handleMethod.Invoke(handler, [request, cancellationToken]);

        if (result is Task<TResponse> taskResult)
        {
            return await taskResult;
        }

        throw new InvalidOperationException($"Handler did not return expected Task<{typeof(TResponse).Name}>");
    }
}