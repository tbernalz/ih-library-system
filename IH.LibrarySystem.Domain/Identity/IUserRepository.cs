namespace IH.LibrarySystem.Domain.Identity;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByGoogleSubjectIdAsync(string googleSubjectId);

    Task AddAsync(User user);
}
