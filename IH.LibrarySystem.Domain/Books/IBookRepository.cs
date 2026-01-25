namespace IH.LibrarySystem.Domain.Books;

public interface IBookRepository
{
    Task<Book?> GetByIdAsync(Guid id);
    Task<Book?> GetByIsbnAsync(string isbn);
    Task<bool> HasBooksByAuthorIdAsync(Guid authorId);

    Task AddAsync(Book book);
    void Delete(Book book);
}
