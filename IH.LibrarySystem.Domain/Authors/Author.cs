using IH.LibrarySystem.Domain.SharedKernel;

namespace IH.LibrarySystem.Domain.Authors;

public class Author : Entity
{
    public string Name { get; private set; } = default!;
    public string? Bio { get; private set; }

    private Author()
        : base(Guid.Empty) { }

    private Author(Guid id, string name, string? bio)
        : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        Bio = bio;
    }

    public static Author Create(Guid id, string name, string? bio = null) => new(id, name, bio);
}
