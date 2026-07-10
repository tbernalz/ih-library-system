namespace IH.LibrarySystem.Domain.Members;

public interface IMemberRepository
{
    Task<Member?> GetByIdAsync(
        Guid id,
        bool readOnly = false,
        CancellationToken cancellationToken = default
    );
    Task<Member?> GetByEmailAsync(
        string email,
        bool readOnly = false,
        CancellationToken cancellationToken = default
    );

    Task AddAsync(Member member);
    void Delete(Member member);
}
