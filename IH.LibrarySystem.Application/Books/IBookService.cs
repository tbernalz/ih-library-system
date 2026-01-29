using IH.LibrarySystem.Application.Books.Dtos;
using IH.LibrarySystem.Domain.Common;

namespace IH.LibrarySystem.Application.Books;

public interface IBookService
{
    Task<BookDto> GetBookByIdAsync(Guid bookId);
    Task<PagedResult<BookDto>> SearchBooksAsync(BookSearchRequest request);

    Task<BookDto> CreateBookAsync(CreateBookRequest request);
    Task<BookDto> UpdateBookAsync(Guid bookId, UpdateBookRequest request);
    Task DeleteBookAsync(Guid bookId);
}
