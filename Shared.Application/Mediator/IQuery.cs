namespace Shared.Application.Mediator;

/// <summary>
/// Marker interface for queries that return a response
/// Queries should not modify state
/// </summary>
/// <typeparam name="TResponse">The type of response</typeparam>
public interface IQuery<TResponse> : IRequest<TResponse>
{
}
