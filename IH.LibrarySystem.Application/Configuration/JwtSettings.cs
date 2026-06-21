using System.ComponentModel.DataAnnotations;

namespace IH.LibrarySystem.Application.Configuration;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    [Required, MinLength(32)]
    public string SigningKey { get; init; } = string.Empty;

    [Required]
    public string Issuer { get; init; } = "ih-library-system";

    [Required]
    public string Audience { get; init; } = "ih-library-system-clients";

    [Range(1, 1440)]
    public int AccessTokenLifetimeMinutes { get; init; } = 15;

    [Range(1, 365)]
    public int RefreshTokenLifetimeDays { get; init; } = 30;
}
