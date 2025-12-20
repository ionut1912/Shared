using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Api.Abstractions;

namespace Shared.Api.Handlers
{
    /// <summary>
    /// Handles exceptions globally for the application and maps them to <see cref="ProblemDetails"/> responses.
    /// </summary>
    public class GlobalExceptionHandler(IEnumerable<IExceptionProblemDetailsMapper> mappers) : IExceptionHandler
    {
        private readonly IEnumerable<IExceptionProblemDetailsMapper> _mappers = mappers;

        /// <summary>
        /// Attempts to handle the specified exception by mapping it to a <see cref="ProblemDetails"/> response.
        /// </summary>
        /// <param name="httpContext">The current HTTP context.</param>
        /// <param name="exception">The exception to handle.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>
        /// <c>true</c> if the exception was handled and a response was written; otherwise, <c>false</c>.
        /// </returns>
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            // Iterate through all registered mappers to attempt mapping the exception
            foreach (var mapper in _mappers)
            {
                if (mapper.TryMap(exception, out var problemDetails))
                {
                    // Set the response status code based on the ProblemDetails
                    httpContext.Response.StatusCode =
                        problemDetails.Status ?? StatusCodes.Status500InternalServerError;

                    // Write the ProblemDetails as JSON response
                    await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
                    return true;
                }
            }

            // Fallback: if no mapper handled the exception, return generic 500 error
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
}
