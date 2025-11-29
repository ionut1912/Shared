using Freelance.Shared.Domain.Common;

namespace Freelance.Shared.Domain.Exceptions;

public class CustomValidationException(List<ValidationError> validationErrors) : Exception
{
    public List<ValidationError> ValidationErrors { get; } = validationErrors;
}