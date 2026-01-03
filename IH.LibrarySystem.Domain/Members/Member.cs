using IH.LibrarySystem.Domain.SharedKernel;

namespace IH.LibrarySystem.Domain.Members;

public class Member : Entity
{
    public string Name { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public DateTime JoinDate { get; private set; }
    public MemberStatus Status { get; private set; }

    private Member()
        : base(Guid.Empty) { }

    private Member(Guid id, string name, string email, DateTime? joinDate = null)
        : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        Name = name;
        Email = email;
        JoinDate = joinDate ?? DateTime.UtcNow;
        Status = MemberStatus.Active;
    }

    public static Member Create(Guid id, string name, string email, DateTime? joinDate = null) =>
        new(id, name, email, joinDate);
}
