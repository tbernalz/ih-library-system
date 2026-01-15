using IH.LibrarySystem.Application.Books;
using IH.LibrarySystem.Application.Books.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace IH.LibrarySystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController(IBookService bookService) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult<BookDto>> GetBook(Guid id)
    {
        var book = await bookService.GetBookByIdAsync(id);
        return Ok(book);
    }

    [HttpPost]
    public async Task<ActionResult<BookDto>> CreateBook(CreateBookRequestDto request)
    {
        var book = await bookService.CreateBookAsync(request);
        return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<BookDto>> UpdateBook(Guid id, UpdateBookRequestDto request)
    {
        var book = await bookService.UpdateBookAsync(id, request);
        return Ok(book);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBook(Guid id)
    {
        await bookService.DeleteBookAsync(id);
        return NoContent();
    }
}
