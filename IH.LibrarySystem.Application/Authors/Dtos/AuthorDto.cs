namespace IH.LibrarySystem.Application.Authors.Dtos;

public record AuthorDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Bio { get; init; }
}
