using IH.LibrarySystem.Application.Authors.Dtos;
using IH.LibrarySystem.Domain.Authors;

namespace IH.LibrarySystem.Application.Authors;

public class AuthorService(IAuthorRepository repository) : IAuthorService
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
        var author = Author.Create(Guid.NewGuid(), request.Name, request.Bio);
        await repository.AddAsync(author);
        await repository.SaveChangesAsync();

        return MapToDto(author);
    }

    public async Task<AuthorDto> UpdateAuthorAsync(Guid authorId, UpdateAuthorRequestDto request)
    {
        var author =
            await repository.GetByIdAsync(authorId)
            ?? throw new KeyNotFoundException($"Author with ID {authorId} not found.");

        author.Update(request.Name, request.Bio);
        repository.Update(author);
        await repository.SaveChangesAsync();

        return MapToDto(author);
    }

    public async Task DeleteAuthorAsync(Guid authorId)
    {
        var author =
            await repository.GetByIdAsync(authorId)
            ?? throw new KeyNotFoundException($"Author with ID {authorId} not found.");

        repository.Delete(author);
        await repository.SaveChangesAsync();
    }

    private static AuthorDto MapToDto(Author author) =>
        new()
        {
            Id = author.Id,
            Name = author.Name,
            Bio = author.Bio,
        };
}
