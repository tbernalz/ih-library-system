using IH.LibrarySystem.Application.Authors.Dtos;

namespace IH.LibrarySystem.Application.Authors;

public interface IAuthorService
{
    Task<AuthorDto> GetAuthorByIdAsync(Guid authorId);

    Task<AuthorDto> CreateAuthorAsync(CreateAuthorRequest request);
    Task<AuthorDto> UpdateAuthorAsync(Guid authorId, UpdateAuthorRequest request);

    Task DeleteAuthorAsync(Guid authorId);
}
