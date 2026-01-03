using IH.LibrarySystem.Domain.Entities;

namespace IH.LibrarySystem.Domain.Repositories;

public interface IBookRepository
{
    Task<Book?> GetByIdAsync(Guid id);

    Task AddAsync(Book book);
    void Update(Book book);
    void Delete(Book book);

    Task SaveChangesAsync();
}
