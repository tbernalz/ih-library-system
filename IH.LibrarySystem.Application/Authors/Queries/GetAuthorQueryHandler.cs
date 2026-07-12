using IH.LibrarySystem.Application.Authors.Dtos;
using IH.LibrarySystem.Application.Common.Exceptions;
using IH.LibrarySystem.Domain.Authors;
using MediatR;

namespace IH.LibrarySystem.Application.Authors.Queries;

public sealed class GetAuthorQueryHandler(IAuthorRepository authorRepository)
    : IRequestHandler<GetAuthorQuery, AuthorDto>
{
    public async Task<AuthorDto> Handle(GetAuthorQuery request, CancellationToken cancellationToken)
    {
        var author = await authorRepository.GetByIdAsync(
            request.Id,
            readOnly: true,
            cancellationToken
        );

        _ = author ?? throw new NotFoundException(nameof(Author), request.Id);

        return new AuthorDto
        {
            Id = author.Id,
            Name = author.Name,
            Email = author.Email,
            Bio = author.Bio,
        };
    }
}
