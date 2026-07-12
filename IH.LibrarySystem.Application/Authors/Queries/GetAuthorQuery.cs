using IH.LibrarySystem.Application.Authors.Dtos;
using MediatR;

namespace IH.LibrarySystem.Application.Authors.Queries;

public record GetAuthorQuery(Guid Id) : IRequest<AuthorDto>;
