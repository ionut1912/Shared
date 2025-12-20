using FluentValidation;
using Shared.Application.Mediator;
using Shared.Domain.Common;
using Shared.Domain.Exceptions;

namespace Shared.Application.Behaviours
{
    /// <summary>
    /// Pipeline behavior that validates requests using FluentValidation before they reach the request handler.
    /// </summary>
    /// <typeparam name="TRequest">The type of request being validated.</typeparam>
    /// <typeparam name="TResponse">The type of response returned by the handler.</typeparam>
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationBehavior{TRequest, TResponse}"/> class.
        /// </summary>
        /// <param name="validators">A collection of validators for the request type.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="validators"/> is null.</exception>
        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            ArgumentNullException.ThrowIfNull(validators, nameof(validators));
            _validators = validators;
        }

        /// <summary>
        /// Handles the validation of the request before invoking the next handler in the pipeline.
        /// </summary>
        /// <param name="request">The request to validate.</param>
        /// <param name="next">The delegate to invoke the next handler in the pipeline.</param>
        /// <param name="cancellationToken">A cancellation token for async operations.</param>
        /// <returns>The response from the next handler if validation succeeds.</returns>
        /// <exception cref="CustomValidationException">
        /// Thrown when one or more validation failures occur.
        /// Contains a list of <see cref="ValidationError"/> objects describing the failures.
        /// </exception>
        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            // If there are no validators, just proceed to the next handler
            if (!_validators.Any())
            {
                return await next();
            }

            var context = new ValidationContext<TRequest>(request);

            // Execute all validators asynchronously
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            // Collect all validation failures
            var failures = validationResults
                .Where(r => !r.IsValid)
                .SelectMany(r => r.Errors)
                .Select(f => new ValidationError(f.PropertyName, f.ErrorMessage))
                .ToList();

            // If any failures exist, throw a custom validation exception
            if (failures.Count > 0)
            {
                throw new CustomValidationException(failures);
            }

            // Otherwise, continue to the next handler
            return await next();
        }
    }
}
