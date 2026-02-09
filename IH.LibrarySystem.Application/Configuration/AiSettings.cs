using System.ComponentModel.DataAnnotations;

namespace IH.LibrarySystem.Application.Configuration;

public class AiSettings
{
    public const string SectionName = "AiSettings";

    [Required]
    public string Provider { get; init; } = "openrouter";

    [Required]
    public string Model { get; init; } = "openai/gpt-oss-120b:free";

    public string? ApiKey { get; init; }

    public string? BaseUrl { get; init; }

    public int MaxTokens { get; init; } = 1000;

    public double Temperature { get; init; } = 0.7;
}
