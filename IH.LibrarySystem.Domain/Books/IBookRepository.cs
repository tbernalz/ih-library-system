namespace IH.LibrarySystem.Domain.Books;

public interface IBookRepository
{
    Task<Book?> GetByIdAsync(Guid id);
    Task<Book?> GetByIsbnAsync(string isbn);

    Task AddAsync(Book book);
    void Delete(Book book);
}
