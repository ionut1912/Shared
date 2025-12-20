namespace Shared.Domain.Common;

/// <summary>
/// Represents a validation failure for a specific property.
/// </summary>
/// <param name="Property">The name of the property that failed validation.</param>
/// <param name="ErrorMessage">The validation error message associated with the property.</param>
public record ValidationError(string Property, string ErrorMessage);
