namespace IH.LibrarySystem.Application.Common.Exceptions;

/// <summary>
/// Thrown when a request requires an authenticated caller but none could be resolved.
/// </summary>
public sealed class UnauthorizedException : ApplicationException
{
    public UnauthorizedException(string message)
        : base(message) { }
}
