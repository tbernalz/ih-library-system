using IH.LibrarySystem.Domain.Identity;

namespace IH.LibrarySystem.Application.Common.Abstractions;

/// <summary>
/// Provides the identity of the caller executing the current application request.
/// Implemented in Infrastructure via <c>IHttpContextAccessor</c>; consumed by Application
/// services so controllers never parse JWT claims themselves.
/// </summary>
public interface ICurrentUserContext
{
    bool IsAuthenticated { get; }

    /// <summary>
    /// The authenticated user's id. Throws <see cref="Exceptions.UnauthorizedException"/>
    /// when the caller is not authenticated or the subject claim is missing/invalid.
    /// </summary>
    Guid UserId { get; }

    /// <summary>
    /// Returns the user id when present; otherwise <c>null</c> without throwing.
    /// </summary>
    Guid? TryGetUserId();

    string? Email { get; }

    UserRole? Role { get; }

    bool IsStaffOrAdmin { get; }
}
