using IH.LibrarySystem.Domain.Enums;
using StandUsers.Domain.SharedKernel;

namespace IH.LibrarySystem.Domain.Entities;

public class Book : Entity
{
    private readonly List<Loan> _loans = [];

    public Guid AuthorId { get; private set; }

    public string Title { get; private set; } = default!;
    public string Isbn { get; private set; } = default!;
    public string Genre { get; private set; } = default!;
    public BookStatus Status { get; private set; }

    public Author? Author { get; private set; }

    public IReadOnlyCollection<Loan> Loans => _loans.AsReadOnly();

    private Book()
        : base(Guid.Empty) { }

    private Book(Guid id, string title, string isbn, Guid authorId, string genre)
        : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(isbn);
        ArgumentException.ThrowIfNullOrWhiteSpace(genre);

        Title = title;
        Isbn = isbn;
        AuthorId = authorId;
        Genre = genre;
        Status = BookStatus.Available;
    }

    public static Book Create(Guid id, string title, string isbn, Guid authorId, string genre) =>
        new(id, title, isbn, authorId, genre);
}
