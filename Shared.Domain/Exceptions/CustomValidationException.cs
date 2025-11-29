using Shared.Domain.Common;

namespace Shared.Domain.Exceptions;

public class CustomValidationException(List<ValidationError> validationErrors) : Exception
{
    public List<ValidationError> ValidationErrors { get; } = validationErrors;
}