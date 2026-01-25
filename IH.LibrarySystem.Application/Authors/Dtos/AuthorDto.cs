using System.ComponentModel.DataAnnotations;

namespace IH.LibrarySystem.Application.Authors.Dtos;

public record AuthorDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }

    [EmailAddress]
    public required string Email { get; init; }
    public string? Bio { get; init; }
}
