namespace IH.LibrarySystem.Domain.Common.Exceptions;

/// <summary>
/// Base exception for all domain-specific business rule violations.
/// Domain exceptions represent violations of invariant business rules and should
/// only be thrown from the Domain layer to maintain architectural purity.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message)
        : base(message) { }

    protected DomainException(string message, Exception innerException)
        : base(message, innerException) { }
}
