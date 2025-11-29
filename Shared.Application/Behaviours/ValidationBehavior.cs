using FluentValidation;
using Freelance.Shared.Domain.Common;
using Freelance.Shared.Domain.Exceptions;
using MediatR;

namespace Shared.Application.Behaviours;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        ArgumentNullException.ThrowIfNull(validators, nameof(validators));
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(x => x.ValidateAsync(context, cancellationToken)));

        var failures = validationResults.Where(x => !x.IsValid)
            .SelectMany(x => x.Errors)
            .Select(x => new ValidationError(x.PropertyName, x.ErrorMessage))
            .ToList();

        if (failures.Count > 0) throw new CustomValidationException(failures);

        var response = await next();
        return response;
    }
}