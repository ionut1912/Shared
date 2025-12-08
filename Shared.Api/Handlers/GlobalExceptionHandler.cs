using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Api.Abstractions;
namespace Shared.Api.Handlers;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IEnumerable<IExceptionProblemDetailsMapper> _mappers;

    public GlobalExceptionHandler(IEnumerable<IExceptionProblemDetailsMapper> mappers)
    {
        _mappers = mappers;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        foreach (var mapper in _mappers)
        {
            if (mapper.TryMap(exception, out var problemDetails))
            {
                httpContext.Response.StatusCode =
                    problemDetails.Status ?? StatusCodes.Status500InternalServerError;

                await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
                return true;
            }
        }

        // Fallback: generic 500
        var fallback = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Internal Server Error",
            Detail = "An unexpected error occurred"
        };

        httpContext.Response.StatusCode = fallback.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(fallback, cancellationToken);

        return true;
    }
}
