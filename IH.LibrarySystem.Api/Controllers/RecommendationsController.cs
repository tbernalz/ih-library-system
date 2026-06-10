using IH.LibrarySystem.Application.Recommendations;
using IH.LibrarySystem.Application.Recommendations.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace IH.LibrarySystem.Api.Controllers;

[ApiController]
[Route("api/members/{memberId:guid}/recommendations")]
public sealed class RecommendationsController(IRecommendationService recommendationService)
    : ControllerBase
{
    /// <summary>
    /// Returns AI-powered book recommendations for a member based on their loan history.
    /// Books the member has already borrowed are excluded.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<RecommendationsResponse>> GetRecommendations(
        Guid memberId,
        [FromQuery] int topK = 5,
        CancellationToken cancellationToken = default
    )
    {
        if (topK is < 1 or > 20)
            return BadRequest("topK must be between 1 and 20.");

        var result = await recommendationService.GetRecommendationsAsync(
            memberId,
            topK,
            cancellationToken
        );

        return Ok(result);
    }
}
