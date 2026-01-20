namespace IH.LibrarySystem.Domain.Authors;

public interface IAuthorRepository
{
    Task<Author?> GetByIdAsync(Guid id);
    Task<Author?> GetByEmailAsync(string email);

    Task AddAsync(Author author);
    void Delete(Author author);
}
