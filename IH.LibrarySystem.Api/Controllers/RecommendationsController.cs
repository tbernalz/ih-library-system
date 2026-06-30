using IH.LibrarySystem.Application.Recommendations;
using IH.LibrarySystem.Application.Recommendations.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IH.LibrarySystem.Api.Controllers;

[ApiController]
[Route("api/members/recommendations")]
public sealed class RecommendationsController(IRecommendationService recommendationService)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<RecommendationsResponse>> GetRecommendations(
        [FromQuery] Guid memberId,
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
