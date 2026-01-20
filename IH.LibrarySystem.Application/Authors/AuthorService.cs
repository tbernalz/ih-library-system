using IH.LibrarySystem.Application.Authors.Dtos;
using IH.LibrarySystem.Domain.Authors;
using IH.LibrarySystem.Domain.SharedKernel;

namespace IH.LibrarySystem.Application.Authors;

public class AuthorService(IAuthorRepository repository, IUnitOfWork unitOfWork) : IAuthorService
{
    public async Task<AuthorDto> GetAuthorByIdAsync(Guid authorId)
    {
        var author =
            await repository.GetByIdAsync(authorId)
            ?? throw new KeyNotFoundException($"Author with ID {authorId} not found.");

        return MapToDto(author);
    }

    public async Task<AuthorDto> CreateAuthorAsync(CreateAuthorRequestDto request)
    {
        var existing = await repository.GetByEmailAsync(request.Email);

        if (existing is not null)
            throw new InvalidOperationException($"Email '{request.Email}' is already registered.");

        var author = Author.Create(Guid.NewGuid(), request.Name, request.Email, request.Bio);
        await repository.AddAsync(author);
        await unitOfWork.SaveChangesAsync();

        return MapToDto(author);
    }

    public async Task<AuthorDto> UpdateAuthorAsync(Guid authorId, UpdateAuthorRequestDto request)
    {
        var author =
            await repository.GetByIdAsync(authorId)
            ?? throw new KeyNotFoundException($"Author with ID {authorId} not found.");

        if (
            request.Name == author.Name
            && request.Bio == author.Bio
            && author.Email == author.Email
        )
            return MapToDto(author);

        if (!string.Equals(author.Email, request.Email, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await repository.GetByEmailAsync(request.Email);
            if (existing is not null)
                throw new InvalidOperationException($"Email '{request.Email}' is already taken.");
        }

        author.Update(request.Name, request.Email, request.Bio);
        await unitOfWork.SaveChangesAsync();

        return MapToDto(author);
    }

    public async Task DeleteAuthorAsync(Guid authorId)
    {
        var author =
            await repository.GetByIdAsync(authorId)
            ?? throw new KeyNotFoundException($"Author with ID {authorId} not found.");

        repository.Delete(author);
        await unitOfWork.SaveChangesAsync();
    }

    private static AuthorDto MapToDto(Author author) =>
        new()
        {
            Id = author.Id,
            Name = author.Name,
            Email = author.Email,
            Bio = author.Bio,
        };
}
