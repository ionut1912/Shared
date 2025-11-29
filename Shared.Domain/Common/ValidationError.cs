namespace Shared.Domain.Common;

public record ValidationError(string Property, string ErrorMessage);