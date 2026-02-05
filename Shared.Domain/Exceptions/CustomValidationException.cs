using Shared.Domain.Common;

namespace Shared.Domain.Exceptions;

/// <summary>
/// Represents an exception that occurs when one or more validation errors are detected.
/// </summary>
public class CustomValidationException : Exception
{
    /// <summary>
    /// Gets the list of validation errors that caused this exception.
    /// </summary>
    public List<ValidationError> ValidationErrors { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomValidationException"/> class
    /// with the specified validation errors.
    /// </summary>
    /// <param name="validationErrors">The collection of <see cref="ValidationError"/> instances that describe the validation failures.</param>
    public CustomValidationException(List<ValidationError> validationErrors)
        : base("One or more validation errors occurred.")
    {
        ValidationErrors = validationErrors ?? new List<ValidationError>();
    }
}
