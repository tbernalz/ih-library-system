using IH.LibrarySystem.Application.Books.Dtos;

namespace IH.LibrarySystem.Application.Books;

public interface IBookService
{
    Task<BookDto> GetBookByIdAsync(Guid bookId);

    Task<BookDto> CreateBookAsync(CreateBookRequest request);
    Task<BookDto> UpdateBookAsync(Guid bookId, UpdateBookRequest request);
    Task DeleteBookAsync(Guid bookId);
}
