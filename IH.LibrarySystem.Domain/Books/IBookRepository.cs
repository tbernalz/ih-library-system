using IH.LibrarySystem.Domain.Common;

namespace IH.LibrarySystem.Domain.Books;

public interface IBookRepository
{
    Task<Book?> GetByIdAsync(Guid id);
    Task<Book?> GetByIsbnAsync(string isbn);
    Task<bool> HasBooksByAuthorIdAsync(Guid authorId);

    Task<PagedResult<Book>> SearchAsync(BookSearchFilter filter);

    Task AddAsync(Book book);
    void Delete(Book book);
}
