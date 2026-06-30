using IH.LibrarySystem.Application.Ai;
using IH.LibrarySystem.Application.Ai.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IH.LibrarySystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AiController(IAiService aiService) : ControllerBase
{
    [HttpPost("complete")]
    public async Task<ActionResult<string>> Complete([FromBody] CompleteRequest request)
    {
        var response = await aiService.CompleteAsync(request);
        return Ok(new { response });
    }

    [HttpPost("summarize-book")]
    public async Task<ActionResult<string>> SummarizeBook([FromBody] SummarizeBookRequest request)
    {
        var summary = await aiService.SummarizeBookDescriptionAsync(request);
        return Ok(new { summary });
    }

    [HttpPost("recommend-books")]
    public async Task<ActionResult<string>> RecommendBooks([FromBody] RecommendBooksRequest request)
    {
        var recommendations = await aiService.RecommendBooksAsync(request);
        return Ok(new { recommendations });
    }
}
