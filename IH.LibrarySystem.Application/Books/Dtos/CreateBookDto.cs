using System.ComponentModel.DataAnnotations;

namespace IH.LibrarySystem.Application.Books.Dtos;

public record CreateBookRequestDto(
    [Required, StringLength(200)] string Title,
    [Required, StringLength(20)] string Isbn,
    [Required, StringLength(100)] string Genre,
    Guid AuthorId
);
