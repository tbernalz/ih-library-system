using System.ComponentModel.DataAnnotations;
using IH.LibrarySystem.Domain.Members;

namespace IH.LibrarySystem.Application.Members.Dtos;

public record UpdateStatusRequest([Required] MemberStatus Status);
