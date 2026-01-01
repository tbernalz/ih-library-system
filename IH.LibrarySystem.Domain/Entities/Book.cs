using IH.LibrarySystem.Domain.Enums;
using StandUsers.Domain.SharedKernel;

namespace IH.LibrarySystem.Domain.Entities;

public class Book : Entity
{
    public string Title { get; private set; } = default!;
    public string Isbn { get; private set; } = default!;
    public string Genre { get; private set; } = default!;
    public BookStatus Status { get; private set; }

    private Book()
        : base(Guid.Empty) { }

    private Book(Guid id, string title, string isbn, string genre)
        : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(isbn);
        ArgumentException.ThrowIfNullOrWhiteSpace(genre);

        Title = title;
        Isbn = isbn;
        Genre = genre;
        Status = BookStatus.Available;
    }

    public static Book Create(Guid id, string title, string isbn, string genre) =>
        new(id, title, isbn, genre);

    public void AssignAuthor(Author author)
    {
        ArgumentNullException.ThrowIfNull(author);

        SetUpdated();
    }
}
