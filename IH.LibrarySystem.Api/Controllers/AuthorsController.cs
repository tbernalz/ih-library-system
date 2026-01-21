using IH.LibrarySystem.Application.Authors;
using IH.LibrarySystem.Application.Authors.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace IH.LibrarySystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthorsController(IAuthorService authorService) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult<AuthorDto>> GetAuthor(Guid id)
    {
        var author = await authorService.GetAuthorByIdAsync(id);
        return Ok(author);
    }

    [HttpPost]
    public async Task<ActionResult<AuthorDto>> CreateAuthor(CreateAuthorRequest request)
    {
        var author = await authorService.CreateAuthorAsync(request);
        return CreatedAtAction(nameof(GetAuthor), new { id = author.Id }, author);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AuthorDto>> UpdateAuthor(Guid id, UpdateAuthorRequest request)
    {
        var author = await authorService.UpdateAuthorAsync(id, request);
        return Ok(author);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAuthor(Guid id)
    {
        await authorService.DeleteAuthorAsync(id);
        return NoContent();
    }
}
