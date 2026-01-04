using System.ComponentModel.DataAnnotations;

namespace IH.LibrarySystem.Application.Authors.Dtos;

public record CreateAuthorRequestDto(
    [Required, StringLength(100)] string Name,
    [StringLength(500)] string? Bio
);
