using System.ComponentModel.DataAnnotations;
using IH.LibrarySystem.Domain.Members;

namespace IH.LibrarySystem.Application.Members.Dtos;

public record MemberDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }

    [EmailAddress]
    public required string Email { get; init; }
    public required DateTime JoinDate { get; init; }
    public required MemberStatus Status { get; init; }
}
