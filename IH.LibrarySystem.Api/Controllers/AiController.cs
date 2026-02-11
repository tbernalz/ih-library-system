using IH.LibrarySystem.Application.Ai;
using IH.LibrarySystem.Application.Ai.Dtos;
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
}
