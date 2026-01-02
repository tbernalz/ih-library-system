using IH.LibrarySystem.Domain.Enums;
using StandUsers.Domain.SharedKernel;

namespace IH.LibrarySystem.Domain.Entities;

public class Member : Entity
{
    private readonly List<Loan> _loans = [];

    public string Name { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public DateTime JoinDate { get; private set; }
    public MemberStatus Status { get; private set; }

    public IReadOnlyCollection<Loan> Loans => _loans.AsReadOnly();

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
