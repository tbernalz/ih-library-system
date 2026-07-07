using IH.LibrarySystem.Application.Recommendations.Dtos;

namespace IH.LibrarySystem.Application.Recommendations;

public interface IRecommendationService
{
    /// <summary>
    /// Returns up to <paramref name="topK"/> AI-ranked book recommendations
    /// for <paramref name="memberId"/> based on their loan history.
    /// Books the member has already borrowed are excluded.
    /// </summary>
    Task<RecommendationsResponse> GetRecommendationsAsync(
        Guid memberId,
        int topK = 5,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Returns up to <paramref name="topK"/> AI-ranked book recommendations
    /// for the currently authenticated user based on their loan history.
    /// Books the user has already borrowed are excluded.
    /// </summary>
    Task<RecommendationsResponse> GetRecommendationsForCurrentUserAsync(
        int topK = 5,
        CancellationToken cancellationToken = default
    );
}
