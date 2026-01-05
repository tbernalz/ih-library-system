using IH.LibrarySystem.Application.Books.Dtos;

namespace IH.LibrarySystem.Application.Books;

public interface IBookService
{
    Task<BookDto?> GetBookByIdAsync(Guid bookId);

    Task<BookDto> CreateBookAsync(CreateBookRequestDto request);
    Task<BookDto> UpdateBookAsync(Guid bookId, UpdateBookRequestDto request);
    Task DeleteBookAsync(Guid bookId);
}
