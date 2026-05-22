using System.ComponentModel.DataAnnotations;

namespace IH.LibrarySystem.Application.Configuration;

public sealed class SendGridSettings
{
    public const string SectionName = "SendGrid";

    [Required]
    public string ApiKey { get; init; } = string.Empty;

    [Required, EmailAddress]
    public string FromAddress { get; init; } = string.Empty;

    [Required]
    public string FromName { get; init; } = "IH Library System";

    public bool UseConsoleSink { get; init; } = false;
}
