namespace IH.LibrarySystem.Domain.Identity;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(
        Guid id,
        bool readOnly = false,
        CancellationToken cancellationToken = default
    );
    Task<User?> GetByGoogleSubjectIdAsync(
        string googleSubjectId,
        bool readOnly = false,
        CancellationToken cancellationToken = default
    );

    Task AddAsync(User user);
}
