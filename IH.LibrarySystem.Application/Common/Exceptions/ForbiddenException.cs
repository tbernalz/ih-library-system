namespace IH.LibrarySystem.Application.Common.Exceptions;

/// <summary>
/// Thrown when the caller is authenticated but lacks permission for the requested action.
/// </summary>
public sealed class ForbiddenException : ApplicationException
{
    public ForbiddenException(string message)
        : base(message) { }
}
