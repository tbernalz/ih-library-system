using IH.LibrarySystem.Application.Discovery;
using IH.LibrarySystem.Application.Discovery.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IH.LibrarySystem.Api.Controllers;

[ApiController]
[Route("api/discovery")]
[Authorize]
public sealed class DiscoveryController(IDiscoveryService discoveryService) : ControllerBase
{
    [HttpPost("chat")]
    public async Task<ActionResult<DiscoveryChatResponse>> Chat(
        [FromBody] DiscoveryChatRequest request,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(request.Query))
            return BadRequest("Query is required.");

        var result = await discoveryService.ChatAsync(request.Query, cancellationToken);
        return Ok(result);
    }
}
