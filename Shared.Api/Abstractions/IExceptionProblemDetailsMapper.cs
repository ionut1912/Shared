using Microsoft.AspNetCore.Mvc;

namespace Shared.Api.Abstractions;

/// <summary>
/// Defines a contract for mapping exceptions to <see cref="ProblemDetails"/> instances.
/// </summary>
public interface IExceptionProblemDetailsMapper
{
    /// <summary>
    /// Attempts to map the specified <paramref name="exception"/> to a <see cref="ProblemDetails"/> instance.
    /// </summary>
    /// <param name="exception">The exception to map.</param>
    /// <param name="problemDetails">When this method returns, contains the mapped <see cref="ProblemDetails"/>, if the mapping succeeded; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if the exception was successfully mapped; otherwise, <c>false</c>.</returns>
    bool TryMap(Exception exception, out ProblemDetails problemDetails);
}