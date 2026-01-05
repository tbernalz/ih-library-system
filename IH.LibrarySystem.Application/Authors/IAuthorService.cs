using IH.LibrarySystem.Application.Authors.Dtos;

namespace IH.LibrarySystem.Application.Authors;

public interface IAuthorService
{
    Task<AuthorDto> GetAuthorByIdAsync(Guid authorId);

    Task<AuthorDto> CreateAuthorAsync(CreateAuthorRequestDto request);
    Task<AuthorDto> UpdateAuthorAsync(Guid authorId, UpdateAuthorRequestDto request);

    Task DeleteAuthorAsync(Guid authorId);
}
