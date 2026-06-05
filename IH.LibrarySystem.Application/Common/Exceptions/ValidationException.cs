namespace IH.LibrarySystem.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when request validation fails.
/// Captures structured validation errors compatible with FluentValidation and API Problem Details.
/// </summary>
public class ValidationException : ApplicationException
{
    public ValidationException()
        : base("One or more validation failures have occurred.")
    {
        Errors = [];
    }

    public ValidationException(string message)
        : base(message)
    {
        Errors = [];
    }

    public ValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
        Errors = [];
    }

    public ValidationException(Dictionary<string, string[]> errors)
        : base("One or more validation failures have occurred.")
    {
        Errors = errors;
    }

    public ValidationException(string message, Dictionary<string, string[]> errors)
        : base(message)
    {
        Errors = errors;
    }

    /// <summary>
    /// Dictionary of property names and their associated error messages.
    /// Compatible with FluentValidation validation results and API Problem Details.
    /// </summary>
    public Dictionary<string, string[]> Errors { get; }

    /// <summary>
    /// Adds a validation error for a specific property.
    /// </summary>
    public void AddError(string propertyName, string errorMessage)
    {
        if (!Errors.TryGetValue(propertyName, out var errorMessages))
        {
            Errors[propertyName] = [errorMessage];
        }
        else
        {
            Errors[propertyName] = [.. errorMessages, errorMessage];
        }
    }

    /// <summary>
    /// Adds multiple validation errors for a specific property.
    /// </summary>
    public void AddErrors(string propertyName, string[] errorMessages)
    {
        if (!Errors.TryGetValue(propertyName, out var existingErrors))
        {
            Errors[propertyName] = errorMessages;
        }
        else
        {
            Errors[propertyName] = [.. existingErrors, .. errorMessages];
        }
    }
}
