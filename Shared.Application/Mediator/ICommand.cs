namespace Shared.Application.Mediator;

/// <summary>
/// Marker interface for commands that return a response
/// </summary>
/// <typeparam name="TResponse">The type of response</typeparam>
public interface ICommand<TResponse> : IRequest<TResponse>
{
}

/// <summary>
/// Marker interface for commands that don't return a response
/// </summary>
public interface ICommand : ICommand<Unit>
{
}
