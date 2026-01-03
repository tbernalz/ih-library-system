namespace IH.LibrarySystem.Domain.Authors;

public interface IAuthorRepository
{
    Task<Author?> GetByIdAsync(Guid id);

    Task AddAsync(Author author);
    void Update(Author author);
    void Delete(Author author);

    Task SaveChangesAsync();
}
