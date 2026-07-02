namespace IH.LibrarySystem.Application.Common.Abstractions;

/// <summary>
/// Exposes HTTP request metadata needed by application services (e.g. audit fields on
/// token issuance) without coupling the Application layer to ASP.NET Core types.
/// </summary>
public interface IClientRequestContext
{
    string? ClientIpAddress { get; }
}
