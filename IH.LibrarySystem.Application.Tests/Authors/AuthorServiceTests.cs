using FluentAssertions;
using IH.LibrarySystem.Application.Authors;
using IH.LibrarySystem.Application.Authors.Dtos;
using IH.LibrarySystem.Domain.Authors;
using IH.LibrarySystem.Domain.Books;
using IH.LibrarySystem.Domain.SharedKernel;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace IH.LibrarySystem.Application.Tests.Authors;

public class AuthorServiceTests
{
    private readonly Mock<IAuthorRepository> _authorRepositoryMock;
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<AuthorService>> _loggerMock;
    private readonly AuthorService _sut;

    public AuthorServiceTests()
    {
        _authorRepositoryMock = new Mock<IAuthorRepository>();
        _bookRepositoryMock = new Mock<IBookRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<AuthorService>>();

        _sut = new AuthorService(
            _authorRepositoryMock.Object,
            _bookRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object
        );
    }

    #region GetAuthorByIdAsync Tests

    [Fact]
    public async Task GetAuthorByIdAsync_WhenAuthorExists_ShouldReturnAuthorDto()
    {
        var authorId = Guid.NewGuid();
        var author = Author.Create(authorId, "Test Author", "author@test.com", "Test bio");

        _authorRepositoryMock.Setup(x => x.GetByIdAsync(authorId)).ReturnsAsync(author);

        var result = await _sut.GetAuthorByIdAsync(authorId);

        result.Should().NotBeNull();
        result.Id.Should().Be(authorId);
        result.Name.Should().Be("Test Author");
        result.Email.Should().Be("author@test.com");
        result.Bio.Should().Be("Test bio");

        _authorRepositoryMock.Verify(x => x.GetByIdAsync(authorId), Times.Once);
    }

    [Fact]
    public async Task GetAuthorByIdAsync_WhenAuthorNotFound_ShouldThrowKeyNotFoundException()
    {
        var authorId = Guid.NewGuid();

        _authorRepositoryMock.Setup(x => x.GetByIdAsync(authorId)).ReturnsAsync((Author?)null);

        Func<Task> act = async () => await _sut.GetAuthorByIdAsync(authorId);

        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Author with ID {authorId} not found.");

        _authorRepositoryMock.Verify(x => x.GetByIdAsync(authorId), Times.Once);
    }

    #endregion

    #region CreateAuthorAsync Tests

    [Fact]
    public async Task CreateAuthorAsync_WithValidRequest_ShouldCreateAuthor()
    {
        var request = new CreateAuthorRequest("New Author", "newauthor@test.com", "New author bio");

        _authorRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync((Author?)null);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.CreateAuthorAsync(request);

        result.Should().NotBeNull();
        result.Name.Should().Be("New Author");
        result.Email.Should().Be("newauthor@test.com");
        result.Bio.Should().Be("New author bio");

        _authorRepositoryMock.Verify(x => x.GetByEmailAsync(request.Email), Times.Once);
        _authorRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Author>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAuthorAsync_WithDuplicateEmail_ShouldThrowInvalidOperationException()
    {
        var existingAuthor = Author.Create(
            Guid.NewGuid(),
            "Existing Author",
            "existing@test.com",
            "Existing bio"
        );
        var request = new CreateAuthorRequest("New Author", "existing@test.com", "New author bio");

        _authorRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync(existingAuthor);

        Func<Task> act = async () => await _sut.CreateAuthorAsync(request);

        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage($"Email '{request.Email}' is already registered.");

        _authorRepositoryMock.Verify(x => x.GetByEmailAsync(request.Email), Times.Once);
        _authorRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Author>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateAuthorAsync_WithNullBio_ShouldCreateAuthorSuccessfully()
    {
        var request = new CreateAuthorRequest("New Author", "newauthor@test.com", null);

        _authorRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync((Author?)null);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.CreateAuthorAsync(request);

        result.Should().NotBeNull();
        result.Name.Should().Be("New Author");
        result.Email.Should().Be("newauthor@test.com");
        result.Bio.Should().BeNull();

        _authorRepositoryMock.Verify(x => x.GetByEmailAsync(request.Email), Times.Once);
        _authorRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Author>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    #endregion

    #region UpdateAuthorAsync Tests

    [Fact]
    public async Task UpdateAuthorAsync_WithValidRequest_ShouldUpdateAuthor()
    {
        var authorId = Guid.NewGuid();
        var author = Author.Create(authorId, "Original Name", "original@test.com", "Original bio");
        var request = new UpdateAuthorRequest("Updated Name", "updated@test.com", "Updated bio");

        _authorRepositoryMock.Setup(x => x.GetByIdAsync(authorId)).ReturnsAsync(author);

        _authorRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync((Author?)null);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.UpdateAuthorAsync(authorId, request);

        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Name");
        result.Email.Should().Be("updated@test.com");
        result.Bio.Should().Be("Updated bio");

        _authorRepositoryMock.Verify(x => x.GetByIdAsync(authorId), Times.Once);
        _authorRepositoryMock.Verify(x => x.GetByEmailAsync(request.Email), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAuthorAsync_WhenAuthorNotFound_ShouldThrowKeyNotFoundException()
    {
        var authorId = Guid.NewGuid();
        var request = new UpdateAuthorRequest("Updated Name", "updated@test.com", "Updated bio");

        _authorRepositoryMock.Setup(x => x.GetByIdAsync(authorId)).ReturnsAsync((Author?)null);

        Func<Task> act = async () => await _sut.UpdateAuthorAsync(authorId, request);

        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Author with ID {authorId} not found.");

        _authorRepositoryMock.Verify(x => x.GetByIdAsync(authorId), Times.Once);
        _authorRepositoryMock.Verify(x => x.GetByEmailAsync(It.IsAny<string>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateAuthorAsync_WithNoChanges_ShouldReturnExistingAuthor()
    {
        var authorId = Guid.NewGuid();
        var author = Author.Create(authorId, "Test Name", "test@test.com", "Test bio");
        var request = new UpdateAuthorRequest("Test Name", "test@test.com", "Test bio");

        _authorRepositoryMock.Setup(x => x.GetByIdAsync(authorId)).ReturnsAsync(author);

        var result = await _sut.UpdateAuthorAsync(authorId, request);

        result.Should().NotBeNull();
        result.Name.Should().Be("Test Name");
        result.Email.Should().Be("test@test.com");
        result.Bio.Should().Be("Test bio");

        _authorRepositoryMock.Verify(x => x.GetByIdAsync(authorId), Times.Once);
        _authorRepositoryMock.Verify(x => x.GetByEmailAsync(It.IsAny<string>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateAuthorAsync_WithDuplicateEmail_ShouldThrowInvalidOperationException()
    {
        var authorId = Guid.NewGuid();
        var otherAuthorId = Guid.NewGuid();
        var author = Author.Create(authorId, "Original Name", "original@test.com", "Original bio");
        var otherAuthor = Author.Create(
            otherAuthorId,
            "Other Author",
            "other@test.com",
            "Other bio"
        );
        var request = new UpdateAuthorRequest("Updated Name", "other@test.com", "Updated bio");

        _authorRepositoryMock.Setup(x => x.GetByIdAsync(authorId)).ReturnsAsync(author);

        _authorRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync(otherAuthor);

        Func<Task> act = async () => await _sut.UpdateAuthorAsync(authorId, request);

        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage($"Email '{request.Email}' is already taken.");

        _authorRepositoryMock.Verify(x => x.GetByIdAsync(authorId), Times.Once);
        _authorRepositoryMock.Verify(x => x.GetByEmailAsync(request.Email), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateAuthorAsync_WithSameEmailCaseInsensitive_ShouldNotThrowException()
    {
        var authorId = Guid.NewGuid();
        var author = Author.Create(authorId, "Original Name", "test@test.com", "Original bio");
        var request = new UpdateAuthorRequest("Updated Name", "TEST@TEST.COM", "Updated bio");

        _authorRepositoryMock.Setup(x => x.GetByIdAsync(authorId)).ReturnsAsync(author);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.UpdateAuthorAsync(authorId, request);

        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Name");
        result.Email.Should().Be("TEST@TEST.COM");

        _authorRepositoryMock.Verify(x => x.GetByIdAsync(authorId), Times.Once);
        _authorRepositoryMock.Verify(x => x.GetByEmailAsync(It.IsAny<string>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    #endregion

    #region DeleteAuthorAsync Tests

    [Fact]
    public async Task DeleteAuthorAsync_WithValidAuthorId_ShouldDeleteAuthor()
    {
        var authorId = Guid.NewGuid();
        var author = Author.Create(authorId, "Test Author", "author@test.com", "Test bio");

        _authorRepositoryMock.Setup(x => x.GetByIdAsync(authorId)).ReturnsAsync(author);

        _bookRepositoryMock.Setup(x => x.HasBooksByAuthorIdAsync(authorId)).ReturnsAsync(false);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        await _sut.DeleteAuthorAsync(authorId);

        _authorRepositoryMock.Verify(x => x.GetByIdAsync(authorId), Times.Once);
        _bookRepositoryMock.Verify(x => x.HasBooksByAuthorIdAsync(authorId), Times.Once);
        _authorRepositoryMock.Verify(x => x.Delete(author), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAuthorAsync_WhenAuthorNotFound_ShouldThrowKeyNotFoundException()
    {
        var authorId = Guid.NewGuid();

        _authorRepositoryMock.Setup(x => x.GetByIdAsync(authorId)).ReturnsAsync((Author?)null);

        Func<Task> act = async () => await _sut.DeleteAuthorAsync(authorId);

        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Author with ID {authorId} not found.");

        _authorRepositoryMock.Verify(x => x.GetByIdAsync(authorId), Times.Once);
        _bookRepositoryMock.Verify(x => x.HasBooksByAuthorIdAsync(It.IsAny<Guid>()), Times.Never);
        _authorRepositoryMock.Verify(x => x.Delete(It.IsAny<Author>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteAuthorAsync_WhenAuthorHasBooks_ShouldThrowInvalidOperationException()
    {
        var authorId = Guid.NewGuid();
        var author = Author.Create(authorId, "Test Author", "author@test.com", "Test bio");

        _authorRepositoryMock.Setup(x => x.GetByIdAsync(authorId)).ReturnsAsync(author);

        _bookRepositoryMock.Setup(x => x.HasBooksByAuthorIdAsync(authorId)).ReturnsAsync(true);

        Func<Task> act = async () => await _sut.DeleteAuthorAsync(authorId);

        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage(
                $"Cannot delete author '{author.Name}' as they have associated books. Please delete or reassign the books first."
            );

        _authorRepositoryMock.Verify(x => x.GetByIdAsync(authorId), Times.Once);
        _bookRepositoryMock.Verify(x => x.HasBooksByAuthorIdAsync(authorId), Times.Once);
        _authorRepositoryMock.Verify(x => x.Delete(It.IsAny<Author>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    #endregion
}
