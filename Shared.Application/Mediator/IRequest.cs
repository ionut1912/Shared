namespace Shared.Application.Mediator;

/// <summary>
/// Marker interface for requests that return a response
/// </summary>
/// <typeparam name="TResponse">The type of response</typeparam>
public interface IRequest<TResponse>
{
}

/// <summary>
/// Marker interface for requests that don't return a response
/// </summary>
public interface IRequest : IRequest<Unit>
{
}
