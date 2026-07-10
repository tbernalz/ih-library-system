using IH.LibrarySystem.Domain.Common;

namespace IH.LibrarySystem.Domain.Books;

public interface IBookRepository
{
    Task<Book?> GetByIdAsync(
        Guid id,
        bool readOnly = false,
        CancellationToken cancellationToken = default
    );
    Task<Book?> GetByIsbnAsync(
        string isbn,
        bool readOnly = false,
        CancellationToken cancellationToken = default
    );
    Task<bool> HasBooksByAuthorIdAsync(
        Guid authorId,
        bool readOnly = false,
        CancellationToken cancellationToken = default
    );

    Task<PagedResult<Book>> SearchAsync(
        BookSearchFilter filter,
        bool readOnly = false,
        CancellationToken cancellationToken = default
    );

    Task AddAsync(Book book);
    void Delete(Book book);
}
