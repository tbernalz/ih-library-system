namespace IH.LibrarySystem.Domain.Members;

public interface IMemberRepository
{
    Task<Member?> GetByIdAsync(Guid id);

    Task AddAsync(Member member);
    void Update(Member member);
    void Delete(Member member);

    Task SaveChangesAsync();
}
