using System.ComponentModel.DataAnnotations;

namespace IH.LibrarySystem.Application.Configuration;

public sealed class DiscoverySettings
{
    public const string SectionName = "Discovery";

    [Required]
    [MinLength(1)]
    public string EmbeddingModel { get; init; } = "openai/text-embedding-3-small";

    [Range(64, 4096)]
    public int EmbeddingDimensions { get; init; } = 1536;

    public bool EnableBackgroundIngestion { get; init; } = true;

    [Range(1, 128)]
    public int IngestionBatchSize { get; init; } = 8;
}
