using IH.LibrarySystem.Domain.SharedKernel;

namespace IH.LibrarySystem.Domain.Authors;

public class Author : Entity
{
    public string Name { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string? Bio { get; private set; }

    private Author()
        : base(Guid.Empty) { }

    private Author(Guid id, string name, string email, string? bio)
        : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        Name = name;
        Email = email;
        Bio = bio;
    }

    public static Author Create(Guid id, string name, string email, string? bio = null) =>
        new(id, name, email, bio);

    public void Update(string name, string email, string? bio)
    {
        if (Name == name && Email == email && Bio == bio)
            return;

        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        Name = name;
        Email = email;
        Bio = bio;
        SetUpdated();
    }
}
