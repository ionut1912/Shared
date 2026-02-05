using Microsoft.AspNetCore.Routing;

namespace Shared.Api.Infrastructure;

/// <summary>
/// Provides a base class for defining endpoint groups in the web API.
/// Implementations should override <see cref="Map"/> to register endpoints with the application's routing system.
/// </summary>
public abstract class EndpointGroup
{
    /// <summary>
    /// Maps endpoints for this group to the specified <see cref="IEndpointRouteBuilder"/>.
    /// </summary>
    /// <param name="app">The endpoint route builder used to configure routes.</param>
    public abstract void Map(IEndpointRouteBuilder app);
}