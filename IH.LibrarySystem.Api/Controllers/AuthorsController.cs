using IH.LibrarySystem.Application.Authors;
using IH.LibrarySystem.Application.Authors.Commands;
using IH.LibrarySystem.Application.Authors.Dtos;
using IH.LibrarySystem.Application.Authors.Queries;
using IH.LibrarySystem.Application.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IH.LibrarySystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthorizationPolicies.StaffOrAdmin)]
public class AuthorsController(IMediator mediator, IAuthorService authorService) : ControllerBase
{
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthorDto>> GetAuthor(Guid id)
    {
        var author = await mediator.Send(new GetAuthorQuery(id));
        return Ok(author);
    }

    [HttpPost]
    public async Task<ActionResult<AuthorDto>> CreateAuthor(CreateAuthorRequest request)
    {
        var author = await mediator.Send(
            new CreateAuthorCommand(request.Name, request.Email, request.Bio)
        );
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
