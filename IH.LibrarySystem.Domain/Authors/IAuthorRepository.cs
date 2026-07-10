namespace IH.LibrarySystem.Domain.Authors;

public interface IAuthorRepository
{
    Task<Author?> GetByIdAsync(
        Guid id,
        bool readOnly = false,
        CancellationToken cancellationToken = default
    );
    Task<Author?> GetByEmailAsync(
        string email,
        bool readOnly = false,
        CancellationToken cancellationToken = default
    );

    Task AddAsync(Author author);
    void Delete(Author author);
}
