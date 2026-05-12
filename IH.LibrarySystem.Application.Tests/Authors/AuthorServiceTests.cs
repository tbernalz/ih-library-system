using FluentAssertions;
using IH.LibrarySystem.Application.Authors;
using IH.LibrarySystem.Application.Authors.Dtos;
using IH.LibrarySystem.Domain.Authors;
using IH.LibrarySystem.Domain.Books;
using IH.LibrarySystem.Domain.SharedKernel;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace IH.LibrarySystem.Application.Tests.Authors;

public class AuthorServiceTests
{
    private readonly IAuthorRepository _authorRepository;
    private readonly IBookRepository _bookRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuthorService> _logger;
    private readonly AuthorService _sut;

    public AuthorServiceTests()
    {
        _authorRepository = Substitute.For<IAuthorRepository>();
        _bookRepository = Substitute.For<IBookRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _logger = Substitute.For<ILogger<AuthorService>>();

        _sut = new AuthorService(_authorRepository, _bookRepository, _unitOfWork, _logger);
    }

    #region GetAuthorByIdAsync Tests

    [Fact]
    public async Task GetAuthorByIdAsync_WhenAuthorExists_ShouldReturnAuthorDto()
    {
        var authorId = Guid.NewGuid();
        var author = Author.Create(authorId, "Test Author", "author@test.com", "Test bio");
        _authorRepository.GetByIdAsync(authorId).Returns(author);

        var result = await _sut.GetAuthorByIdAsync(authorId);

        result.Should().NotBeNull();
        result.Id.Should().Be(authorId);
        result.Name.Should().Be("Test Author");
    }

    [Fact]
    public async Task GetAuthorByIdAsync_WhenAuthorNotFound_ShouldThrowKeyNotFoundException()
    {
        var authorId = Guid.NewGuid();
        _authorRepository.GetByIdAsync(authorId).Returns((Author?)null);

        var act = () => _sut.GetAuthorByIdAsync(authorId);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    #endregion

    #region CreateAuthorAsync Tests

    [Fact]
    public async Task CreateAuthorAsync_WithValidRequest_ShouldCreateAuthor()
    {
        var request = new CreateAuthorRequest("New Author", "newauthor@test.com", "Bio");
        _authorRepository.GetByEmailAsync(request.Email).Returns((Author?)null);

        var result = await _sut.CreateAuthorAsync(request);

        result.Name.Should().Be(request.Name);

        await _authorRepository.Received(1).AddAsync(Arg.Any<Author>());
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task CreateAuthorAsync_WithDuplicateEmail_ShouldThrowInvalidOperationException()
    {
        var existingAuthor = Author.Create(Guid.NewGuid(), "Existing", "dup@test.com", "Bio");
        var request = new CreateAuthorRequest("New", "dup@test.com", "Bio");
        _authorRepository.GetByEmailAsync(request.Email).Returns(existingAuthor);

        var act = () => _sut.CreateAuthorAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>();
        await _authorRepository.DidNotReceive().AddAsync(Arg.Any<Author>());
    }

    #endregion

    #region UpdateAuthorAsync Tests

    [Fact]
    public async Task UpdateAuthorAsync_WithNoChanges_ShouldReturnExistingAuthor_AndNotSave()
    {
        var authorId = Guid.NewGuid();
        var author = Author.Create(authorId, "Same", "same@test.com", "Same bio");
        var request = new UpdateAuthorRequest("Same", "same@test.com", "Same bio");
        _authorRepository.GetByIdAsync(authorId).Returns(author);

        await _sut.UpdateAuthorAsync(authorId, request);

        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }

    #endregion

    #region DeleteAuthorAsync Tests

    [Fact]
    public async Task DeleteAuthorAsync_WhenAuthorHasBooks_ShouldThrowInvalidOperationException()
    {
        var authorId = Guid.NewGuid();
        var author = Author.Create(authorId, "Author", "a@a.com", "Bio");
        _authorRepository.GetByIdAsync(authorId).Returns(author);
        _bookRepository.HasBooksByAuthorIdAsync(authorId).Returns(true);

        var act = () => _sut.DeleteAuthorAsync(authorId);

        await act.Should().ThrowAsync<InvalidOperationException>();
        _authorRepository.DidNotReceive().Delete(Arg.Any<Author>());
    }

    #endregion
}
