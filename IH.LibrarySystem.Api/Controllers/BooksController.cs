using IH.LibrarySystem.Application.Books;
using IH.LibrarySystem.Application.Books.Dtos;
using IH.LibrarySystem.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IH.LibrarySystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BooksController(IBookService bookService) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult<BookDto>> GetBook(Guid id)
    {
        var book = await bookService.GetBookByIdAsync(id);
        return Ok(book);
    }

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<BookDto>>> SearchBooks(
        [FromQuery] BookSearchRequest request
    )
    {
        var result = await bookService.SearchBooksAsync(request);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "StaffOrAdmin")]
    public async Task<ActionResult<BookDto>> CreateBook(CreateBookRequest request)
    {
        var book = await bookService.CreateBookAsync(request);
        return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "StaffOrAdmin")]
    public async Task<ActionResult<BookDto>> UpdateBook(Guid id, UpdateBookRequest request)
    {
        var book = await bookService.UpdateBookAsync(id, request);
        return Ok(book);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "StaffOrAdmin")]
    public async Task<IActionResult> DeleteBook(Guid id)
    {
        await bookService.DeleteBookAsync(id);
        return NoContent();
    }
}
