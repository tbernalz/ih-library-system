using System.Security.Claims;
using IH.LibrarySystem.Application.Common.Abstractions;
using IH.LibrarySystem.Application.Common.Exceptions;
using IH.LibrarySystem.Domain.Identity;
using Microsoft.AspNetCore.Http;

namespace IH.LibrarySystem.Infrastructure.Identity;

/// <summary>
/// Resolves the current caller from the ASP.NET Core <see cref="HttpContext"/> principal.
/// </summary>
public sealed class HttpContextUserContext(IHttpContextAccessor httpContextAccessor)
    : ICurrentUserContext,
        IClientRequestContext
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

    public Guid UserId =>
        TryGetUserId()
        ?? throw new UnauthorizedException("Access token is missing a valid subject claim.");

    public Guid? TryGetUserId()
    {
        var user = User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var sub = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");

        return sub is not null && Guid.TryParse(sub, out var userId) ? userId : null;
    }

    public string? Email => User?.FindFirstValue(ClaimTypes.Email);

    public UserRole? Role
    {
        get
        {
            var roleValue = User?.FindFirstValue(ClaimTypes.Role);
            return roleValue is not null && Enum.TryParse<UserRole>(roleValue, out var role)
                ? role
                : null;
        }
    }

    public bool IsStaffOrAdmin => Role is UserRole.Staff or UserRole.Admin;

    public string? ClientIpAddress =>
        httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
}
