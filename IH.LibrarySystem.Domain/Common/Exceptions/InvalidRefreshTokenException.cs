namespace IH.LibrarySystem.Domain.Common.Exceptions;

public class InvalidRefreshTokenException : DomainException
{
    public InvalidRefreshTokenException(string reason)
        : base($"Refresh token is invalid: {reason}") { }

    public InvalidRefreshTokenException(string reason, Exception innerException)
        : base($"Refresh token is invalid: {reason}", innerException) { }
}
