using System.ComponentModel.DataAnnotations;

namespace IH.LibrarySystem.Application.Configuration;

public sealed class GoogleAuthSettings
{
    public const string SectionName = "GoogleAuth";

    [Required]
    public string ClientId { get; init; } = string.Empty;
}
