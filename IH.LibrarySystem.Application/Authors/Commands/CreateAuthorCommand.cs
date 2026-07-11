using IH.LibrarySystem.Application.Authors.Dtos;
using MediatR;

namespace IH.LibrarySystem.Application.Authors.Commands;

public record CreateAuthorCommand(string Name, string Email, string? Bio) : IRequest<AuthorDto>;
