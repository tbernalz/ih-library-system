using System.ComponentModel.DataAnnotations;

namespace IH.LibrarySystem.Application.Authors.Dtos;

public record UpdateAuthorRequestDto(
    [Required, StringLength(100)] string Name,
    [Required, EmailAddress, StringLength(255)] string Email,
    [StringLength(500)] string? Bio
);
