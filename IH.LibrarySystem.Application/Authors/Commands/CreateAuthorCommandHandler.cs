using IH.LibrarySystem.Application.Authors.Dtos;
using IH.LibrarySystem.Application.Common.Exceptions;
using IH.LibrarySystem.Domain.Authors;
using IH.LibrarySystem.Domain.SharedKernel;
using MediatR;

namespace IH.LibrarySystem.Application.Authors.Commands;

public sealed class CreateAuthorCommandHandler(
    IAuthorRepository authorRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateAuthorCommand, AuthorDto>
{
    public async Task<AuthorDto> Handle(
        CreateAuthorCommand request,
        CancellationToken cancellationToken
    )
    {
        var existingAuthor = await authorRepository.GetByEmailAsync(
            request.Email,
            readOnly: true,
            cancellationToken
        );

        if (existingAuthor is not null)
        {
            var validationException = new ValidationException();
            validationException.AddError(
                "Email",
                $"Email '{request.Email}' is already registered."
            );
            throw validationException;
        }

        var author = Author.Create(
            id: Guid.NewGuid(),
            name: request.Name,
            email: request.Email,
            bio: request.Bio
        );

        await authorRepository.AddAsync(author);
        await unitOfWork.SaveChangesAsync();

        return new AuthorDto
        {
            Id = author.Id,
            Name = author.Name,
            Email = author.Email,
            Bio = author.Bio,
        };
    }
}
