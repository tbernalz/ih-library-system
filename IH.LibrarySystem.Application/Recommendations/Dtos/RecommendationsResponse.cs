namespace IH.LibrarySystem.Application.Recommendations.Dtos;

public sealed record RecommendationsResponse(
    /// <summary>
    /// AI-generated summary of the member's inferred reading taste.
    /// </summary>
    string ProfileSummary,
    /// <summary>
    /// Ranked list of recommended books with per-book explanations.
    /// </summary>
    IReadOnlyList<RecommendationDto> Recommendations
);
