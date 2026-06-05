namespace IH.LibrarySystem.Application.Common.Exceptions;

/// <summary>
/// Base exception for all application-specific errors.
/// Application exceptions handle cross-cutting orchestration errors such as
/// resource not found, validation failures, and other application-level concerns.
/// </summary>
public abstract class ApplicationException : Exception
{
    protected ApplicationException(string message)
        : base(message) { }

    protected ApplicationException(string message, Exception innerException)
        : base(message, innerException) { }
}
