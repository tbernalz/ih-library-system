using IH.LibrarySystem.Application.Authors.Dtos;
using IH.LibrarySystem.Domain.Authors;
using IH.LibrarySystem.Domain.SharedKernel;
using Microsoft.Extensions.Logging;

namespace IH.LibrarySystem.Application.Authors;

public class AuthorService(
    IAuthorRepository repository,
    IUnitOfWork unitOfWork,
    ILogger<AuthorService> logger
) : IAuthorService
{
    public async Task<AuthorDto> GetAuthorByIdAsync(Guid authorId)
    {
        logger.LogDebug("Fetching author with {AuthorId}", authorId);

        var author = await repository.GetByIdAsync(authorId);
        if (author is null)
        {
            logger.LogWarning("Author retrieval failed: ID {AuthorId} not found", authorId);
            throw new KeyNotFoundException($"Author with ID {authorId} not found.");
        }

        return MapToDto(author);
    }

    public async Task<AuthorDto> CreateAuthorAsync(CreateAuthorRequest request)
    {
        logger.LogInformation("Initiating author creation: request {request}", request);

        var existing = await repository.GetByEmailAsync(request.Email);

        if (existing is not null)
        {
            logger.LogWarning("CreateBook rejected: Duplicate email: {Email}", request.Email);
            throw new InvalidOperationException($"Email '{request.Email}' is already registered.");
        }

        var author = Author.Create(Guid.NewGuid(), request.Name, request.Email, request.Bio);
        await repository.AddAsync(author);
        await unitOfWork.SaveChangesAsync();

        return MapToDto(author);
    }

    public async Task<AuthorDto> UpdateAuthorAsync(Guid authorId, UpdateAuthorRequest request)
    {
        logger.LogInformation(
            "Initiating update for author: {AuthorId}, request {request}",
            authorId,
            request
        );

        var author = await repository.GetByIdAsync(authorId);
        if (author is null)
        {
            logger.LogWarning("UpdateAuthor failed: Author {AuthorId} not found", authorId);
            throw new KeyNotFoundException($"Author with ID {authorId} not found.");
        }

        if (
            request.Name == author.Name
            && request.Bio == author.Bio
            && author.Email == author.Email
        )
        {
            logger.LogDebug("No changes detected for author: {AuthorId}", authorId);
            return MapToDto(author);
        }

        if (!string.Equals(author.Email, request.Email, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await repository.GetByEmailAsync(request.Email);
            if (existing is not null)
            {
                logger.LogWarning(
                    "Attempted to update author {AuthorId} with duplicate email: {Email}",
                    authorId,
                    request.Email
                );
                throw new InvalidOperationException($"Email '{request.Email}' is already taken.");
            }
        }

        author.Update(request.Name, request.Email, request.Bio);
        await unitOfWork.SaveChangesAsync();

        return MapToDto(author);
    }

    public async Task DeleteAuthorAsync(Guid authorId)
    {
        logger.LogInformation("Initiating author deletion: {AuthorId}", authorId);

        var author = await repository.GetByIdAsync(authorId);
        if (author is null)
        {
            logger.LogWarning("DeleteAutho failed: Author {AuthorId} not found", authorId);
            throw new KeyNotFoundException($"Author with ID {authorId} not found.");
        }

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
