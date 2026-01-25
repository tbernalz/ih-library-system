using IH.LibrarySystem.Application.Authors.Dtos;
using IH.LibrarySystem.Domain.Books;

namespace IH.LibrarySystem.Application.Books.Dtos;

public record BookDto
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Isbn { get; init; }
    public BookStatus Status { get; init; }
    public required Guid AuthorId { get; init; }
}
