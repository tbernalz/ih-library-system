using IH.LibrarySystem.Domain.SharedKernel;

namespace IH.LibrarySystem.Domain.Books;

public class Book : Entity
{
    public Guid AuthorId { get; private set; }

    public string Title { get; private set; } = default!;
    public string Isbn { get; private set; } = default!;
    public string Genre { get; private set; } = default!;
    public BookStatus Status { get; private set; }

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

    public void AssignAuthor(Guid authorId)
    {
        if (authorId == Guid.Empty)
            throw new ArgumentException("AuthorId cannot be empty.", nameof(authorId));

        if (AuthorId != authorId)
        {
            AuthorId = authorId;
            SetUpdated();
        }
    }

    public void ChangeMetadata(string title, string isbn, string genre)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(isbn);
        ArgumentException.ThrowIfNullOrWhiteSpace(genre);

        Title = title;
        Isbn = isbn;
        Genre = genre;

        SetUpdated();
    }
}
