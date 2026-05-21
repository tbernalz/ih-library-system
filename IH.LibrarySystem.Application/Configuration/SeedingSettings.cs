using System.ComponentModel.DataAnnotations;

namespace IH.LibrarySystem.Application.Configuration;

public sealed class SeedingSettings
{
    public const string SectionName = "SeedingSettings";

    /// <summary>
    /// Email domain used for seeded/test data. Notifications will not be sent to emails with this domain.
    /// Example: "@seeded.ihlibrary.local"
    /// </summary>
    [Required]
    public string SeededEmailDomain { get; init; } = string.Empty;
}
