using System.ComponentModel.DataAnnotations;

namespace IH.LibrarySystem.Application.Members.Dtos;

public record UpdateMemberRequest(
    [Required] [StringLength(100)] string Name,
    [Required] [EmailAddress] string Email
);
