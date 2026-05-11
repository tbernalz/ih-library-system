using FluentAssertions;
using IH.LibrarySystem.Application.Books;
using IH.LibrarySystem.Application.Books.Dtos;
using IH.LibrarySystem.Domain.Authors;
using IH.LibrarySystem.Domain.Books;
using IH.LibrarySystem.Domain.Common;
using IH.LibrarySystem.Domain.SharedKernel;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace IH.LibrarySystem.Application.Tests.Books;

public class BookServiceTests
{
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly Mock<IAuthorRepository> _authorRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<BookService>> _loggerMock;
    private readonly BookService _sut;

    public BookServiceTests()
    {
        _bookRepositoryMock = new Mock<IBookRepository>();
        _authorRepositoryMock = new Mock<IAuthorRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<BookService>>();

        _sut = new BookService(
            _bookRepositoryMock.Object,
            _authorRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object
        );
    }

    #region GetBookByIdAsync Tests

    [Fact]
    public async Task GetBookByIdAsync_WhenBookExists_ShouldReturnBookDto()
    {
        var bookId = Guid.NewGuid();
        var book = Book.Create(bookId, "Test Book", "1234567890", Guid.NewGuid(), "Fiction");

        _bookRepositoryMock.Setup(x => x.GetByIdAsync(bookId)).ReturnsAsync(book);

        var result = await _sut.GetBookByIdAsync(bookId);

        result.Should().NotBeNull();
        result.Id.Should().Be(bookId);
        result.Title.Should().Be("Test Book");
        result.Isbn.Should().Be("1234567890");
        result.Status.Should().Be(BookStatus.Available);

        _bookRepositoryMock.Verify(x => x.GetByIdAsync(bookId), Times.Once);
    }

    [Fact]
    public async Task GetBookByIdAsync_WhenBookNotFound_ShouldThrowKeyNotFoundException()
    {
        var bookId = Guid.NewGuid();

        _bookRepositoryMock.Setup(x => x.GetByIdAsync(bookId)).ReturnsAsync((Book?)null);

        Func<Task> act = async () => await _sut.GetBookByIdAsync(bookId);

        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Book with ID {bookId} not found.");

        _bookRepositoryMock.Verify(x => x.GetByIdAsync(bookId), Times.Once);
    }

    #endregion

    #region SearchBooksAsync Tests

    [Fact]
    public async Task SearchBooksAsync_WithValidRequest_ShouldReturnPagedResult()
    {
        var request = new BookSearchRequest("test", 1, 10);
        var filter = new BookSearchFilter("test", 1, 10);
        var books = new List<Book>
        {
            Book.Create(Guid.NewGuid(), "Test Book 1", "1234567890", Guid.NewGuid(), "Fiction"),
            Book.Create(Guid.NewGuid(), "Test Book 2", "0987654321", Guid.NewGuid(), "Non-Fiction"),
        };
        var pagedResult = new PagedResult<Book>(books, 2, 1, 10);

        _bookRepositoryMock
            .Setup(x =>
                x.SearchAsync(
                    It.Is<BookSearchFilter>(f =>
                        f.SearchTerm == "test" && f.PageNumber == 1 && f.PageSize == 10
                    )
                )
            )
            .ReturnsAsync(pagedResult);

        var result = await _sut.SearchBooksAsync(request);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Items.First().Title.Should().Be("Test Book 1");

        _bookRepositoryMock.Verify(x => x.SearchAsync(It.IsAny<BookSearchFilter>()), Times.Once);
    }

    [Fact]
    public async Task SearchBooksAsync_WithEmptySearchTerm_ShouldReturnAllBooks()
    {
        var request = new BookSearchRequest(null, 1, 5);
        var books = new List<Book>();
        var pagedResult = new PagedResult<Book>(books, 0, 1, 5);

        _bookRepositoryMock
            .Setup(x => x.SearchAsync(It.IsAny<BookSearchFilter>()))
            .ReturnsAsync(pagedResult);

        var result = await _sut.SearchBooksAsync(request);

        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);

        _bookRepositoryMock.Verify(x => x.SearchAsync(It.IsAny<BookSearchFilter>()), Times.Once);
    }

    #endregion

    #region CreateBookAsync Tests

    [Fact]
    public async Task CreateBookAsync_WithValidRequest_ShouldCreateBook()
    {
        var authorId = Guid.NewGuid();
        var author = Author.Create(authorId, "Test Author", "author@test.com", "Test bio");
        var request = new CreateBookRequest("New Book", "1234567890", "Fiction", authorId);

        _bookRepositoryMock.Setup(x => x.GetByIsbnAsync(request.Isbn)).ReturnsAsync((Book?)null);

        _authorRepositoryMock.Setup(x => x.GetByIdAsync(authorId)).ReturnsAsync(author);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.CreateBookAsync(request);

        result.Should().NotBeNull();
        result.Title.Should().Be("New Book");
        result.Isbn.Should().Be("1234567890");
        result.AuthorId.Should().Be(authorId);
        result.Status.Should().Be(BookStatus.Available);

        _bookRepositoryMock.Verify(x => x.GetByIsbnAsync(request.Isbn), Times.Once);
        _authorRepositoryMock.Verify(x => x.GetByIdAsync(authorId), Times.Once);
        _bookRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Book>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateBookAsync_WithDuplicateIsbn_ShouldThrowInvalidOperationException()
    {
        var authorId = Guid.NewGuid();
        var existingBook = Book.Create(
            Guid.NewGuid(),
            "Existing Book",
            "1234567890",
            authorId,
            "Fiction"
        );
        var request = new CreateBookRequest("New Book", "1234567890", "Fiction", authorId);

        _bookRepositoryMock.Setup(x => x.GetByIsbnAsync(request.Isbn)).ReturnsAsync(existingBook);

        Func<Task> act = async () => await _sut.CreateBookAsync(request);

        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage($"Book with ISBN {request.Isbn} already exists.");

        _bookRepositoryMock.Verify(x => x.GetByIsbnAsync(request.Isbn), Times.Once);
        _authorRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _bookRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Book>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateBookAsync_WithNonExistentAuthor_ShouldThrowKeyNotFoundException()
    {
        var authorId = Guid.NewGuid();
        var request = new CreateBookRequest("New Book", "1234567890", "Fiction", authorId);

        _bookRepositoryMock.Setup(x => x.GetByIsbnAsync(request.Isbn)).ReturnsAsync((Book?)null);

        _authorRepositoryMock.Setup(x => x.GetByIdAsync(authorId)).ReturnsAsync((Author?)null);

        Func<Task> act = async () => await _sut.CreateBookAsync(request);

        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Author with ID {authorId} not found.");

        _bookRepositoryMock.Verify(x => x.GetByIsbnAsync(request.Isbn), Times.Once);
        _authorRepositoryMock.Verify(x => x.GetByIdAsync(authorId), Times.Once);
        _bookRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Book>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    #endregion

    #region UpdateBookAsync Tests

    [Fact]
    public async Task UpdateBookAsync_WithValidRequest_ShouldUpdateBook()
    {
        var bookId = Guid.NewGuid();
        var book = Book.Create(bookId, "Original Title", "1234567890", Guid.NewGuid(), "Fiction");
        var request = new UpdateBookRequest("Updated Title", "0987654321", "Non-Fiction");

        _bookRepositoryMock.Setup(x => x.GetByIdAsync(bookId)).ReturnsAsync(book);

        _bookRepositoryMock.Setup(x => x.GetByIsbnAsync(request.Isbn)).ReturnsAsync((Book?)null);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.UpdateBookAsync(bookId, request);

        result.Should().NotBeNull();
        result.Title.Should().Be("Updated Title");
        result.Isbn.Should().Be("0987654321");
        result.AuthorId.Should().Be(book.AuthorId);

        _bookRepositoryMock.Verify(x => x.GetByIdAsync(bookId), Times.Once);
        _bookRepositoryMock.Verify(x => x.GetByIsbnAsync(request.Isbn), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateBookAsync_WhenBookNotFound_ShouldThrowKeyNotFoundException()
    {
        var bookId = Guid.NewGuid();
        var request = new UpdateBookRequest("Updated Title", "0987654321", "Non-Fiction");

        _bookRepositoryMock.Setup(x => x.GetByIdAsync(bookId)).ReturnsAsync((Book?)null);

        Func<Task> act = async () => await _sut.UpdateBookAsync(bookId, request);

        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Book with ID {bookId} not found.");

        _bookRepositoryMock.Verify(x => x.GetByIdAsync(bookId), Times.Once);
        _bookRepositoryMock.Verify(x => x.GetByIsbnAsync(It.IsAny<string>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateBookAsync_WithNoChanges_ShouldReturnExistingBook()
    {
        var bookId = Guid.NewGuid();
        var book = Book.Create(bookId, "Test Title", "1234567890", Guid.NewGuid(), "Fiction");
        var request = new UpdateBookRequest("Test Title", "1234567890", "Fiction");

        _bookRepositoryMock.Setup(x => x.GetByIdAsync(bookId)).ReturnsAsync(book);

        var result = await _sut.UpdateBookAsync(bookId, request);

        result.Should().NotBeNull();
        result.Title.Should().Be("Test Title");
        result.Isbn.Should().Be("1234567890");

        _bookRepositoryMock.Verify(x => x.GetByIdAsync(bookId), Times.Once);
        _bookRepositoryMock.Verify(x => x.GetByIsbnAsync(It.IsAny<string>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateBookAsync_WithDuplicateIsbn_ShouldThrowInvalidOperationException()
    {
        var bookId = Guid.NewGuid();
        var otherBookId = Guid.NewGuid();
        var book = Book.Create(bookId, "Original Title", "1234567890", Guid.NewGuid(), "Fiction");
        var otherBook = Book.Create(
            otherBookId,
            "Other Book",
            "0987654321",
            Guid.NewGuid(),
            "Fiction"
        );
        var request = new UpdateBookRequest("Updated Title", "0987654321", "Non-Fiction");

        _bookRepositoryMock.Setup(x => x.GetByIdAsync(bookId)).ReturnsAsync(book);

        _bookRepositoryMock.Setup(x => x.GetByIsbnAsync(request.Isbn)).ReturnsAsync(otherBook);

        Func<Task> act = async () => await _sut.UpdateBookAsync(bookId, request);

        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage($"ISBN '{request.Isbn}' is taken.");

        _bookRepositoryMock.Verify(x => x.GetByIdAsync(bookId), Times.Once);
        _bookRepositoryMock.Verify(x => x.GetByIsbnAsync(request.Isbn), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateBookAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        var bookId = Guid.NewGuid();

        Func<Task> act = async () => await _sut.UpdateBookAsync(bookId, null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region DeleteBookAsync Tests

    [Fact]
    public async Task DeleteBookAsync_WithValidBookId_ShouldDeleteBook()
    {
        var bookId = Guid.NewGuid();
        var book = Book.Create(bookId, "Test Book", "1234567890", Guid.NewGuid(), "Fiction");

        _bookRepositoryMock.Setup(x => x.GetByIdAsync(bookId)).ReturnsAsync(book);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        await _sut.DeleteBookAsync(bookId);

        _bookRepositoryMock.Verify(x => x.GetByIdAsync(bookId), Times.Once);
        _bookRepositoryMock.Verify(x => x.Delete(book), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteBookAsync_WhenBookNotFound_ShouldThrowKeyNotFoundException()
    {
        var bookId = Guid.NewGuid();

        _bookRepositoryMock.Setup(x => x.GetByIdAsync(bookId)).ReturnsAsync((Book?)null);

        Func<Task> act = async () => await _sut.DeleteBookAsync(bookId);

        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Book with ID {bookId} not found.");

        _bookRepositoryMock.Verify(x => x.GetByIdAsync(bookId), Times.Once);
        _bookRepositoryMock.Verify(x => x.Delete(It.IsAny<Book>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    #endregion
}
