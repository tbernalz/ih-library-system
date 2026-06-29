using IH.LibrarySystem.Application.Authors;
using IH.LibrarySystem.Application.Authors.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IH.LibrarySystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthorsController(IAuthorService authorService) : ControllerBase
{
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthorDto>> GetAuthor(Guid id)
    {
        var author = await authorService.GetAuthorByIdAsync(id);
        return Ok(author);
    }

    [HttpPost]
    [Authorize(Policy = "StaffOrAdmin")]
    public async Task<ActionResult<AuthorDto>> CreateAuthor(CreateAuthorRequest request)
    {
        var author = await authorService.CreateAuthorAsync(request);
        return CreatedAtAction(nameof(GetAuthor), new { id = author.Id }, author);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "StaffOrAdmin")]
    public async Task<ActionResult<AuthorDto>> UpdateAuthor(Guid id, UpdateAuthorRequest request)
    {
        var author = await authorService.UpdateAuthorAsync(id, request);
        return Ok(author);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "StaffOrAdmin")]
    public async Task<IActionResult> DeleteAuthor(Guid id)
    {
        await authorService.DeleteAuthorAsync(id);
        return NoContent();
    }
}
