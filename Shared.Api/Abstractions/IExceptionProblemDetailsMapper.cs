using Microsoft.AspNetCore.Mvc;

namespace Shared.Api.Abstractions;

public interface IExceptionProblemDetailsMapper
{
    bool TryMap(Exception exception, out ProblemDetails problemDetails);
}