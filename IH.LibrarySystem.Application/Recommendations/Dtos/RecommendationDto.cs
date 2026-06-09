using IH.LibrarySystem.Application.Books.Dtos;

namespace IH.LibrarySystem.Application.Recommendations.Dtos;

public sealed record RecommendationDto
{
    public required BookDto Book { get; init; }

    /// <summary>
    /// Cosine similarity score (0–1). Higher is more relevant.
    /// </summary>
    public required float SimilarityScore { get; init; }

    /// <summary>
    /// One-sentence AI-generated explanation of why this book suits the member.
    /// </summary>
    public required string Reason { get; init; }
}
