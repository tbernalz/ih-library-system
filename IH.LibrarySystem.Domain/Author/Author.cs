using StandUsers.Domain.SharedKernel;

namespace IH.LibrarySystem.Domain.Entities;

public class Author : Entity
{
    private readonly List<Book> _books = [];

    public string Name { get; private set; } = default!;
    public string? Bio { get; private set; }

    public IReadOnlyCollection<Book> Books => _books.AsReadOnly();

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
