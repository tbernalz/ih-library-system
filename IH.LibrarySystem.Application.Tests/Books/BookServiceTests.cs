using FluentAssertions;
using IH.LibrarySystem.Application.Books;
using IH.LibrarySystem.Application.Books.Dtos;
using IH.LibrarySystem.Domain.Authors;
using IH.LibrarySystem.Domain.Books;
using IH.LibrarySystem.Domain.Common;
using IH.LibrarySystem.Domain.SharedKernel;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace IH.LibrarySystem.Application.Tests.Books;

public class BookServiceTests
{
    private readonly IBookRepository _bookRepository;
    private readonly IAuthorRepository _authorRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BookService> _logger;
    private readonly BookService _sut;

    public BookServiceTests()
    {
        _bookRepository = Substitute.For<IBookRepository>();
        _authorRepository = Substitute.For<IAuthorRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _logger = Substitute.For<ILogger<BookService>>();

        _sut = new BookService(_bookRepository, _authorRepository, _unitOfWork, _logger);
    }

    #region GetBookByIdAsync Tests

    [Fact]
    public async Task GetBookByIdAsync_WhenBookExists_ShouldReturnBookDto()
    {
        var bookId = Guid.NewGuid();
        var book = CreateDummyBook(bookId);
        _bookRepository.GetByIdAsync(bookId).Returns(book);

        var result = await _sut.GetBookByIdAsync(bookId);

        result.Should().NotBeNull();
        result.Id.Should().Be(bookId);
        result.Title.Should().Be(book.Title);
    }

    [Fact]
    public async Task GetBookByIdAsync_WhenBookNotFound_ShouldThrowKeyNotFoundException()
    {
        var bookId = Guid.NewGuid();
        _bookRepository.GetByIdAsync(bookId).Returns((Book?)null);

        var act = () => _sut.GetBookByIdAsync(bookId);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    #endregion

    #region SearchBooksAsync Tests

    [Fact]
    public async Task SearchBooksAsync_WithValidRequest_ShouldReturnPagedResult()
    {
        var request = new BookSearchRequest("test", 1, 10);
        var books = new List<Book> { CreateDummyBook(), CreateDummyBook() };
        var pagedResult = new PagedResult<Book>(books, 2, 1, 10);

        _bookRepository
            .SearchAsync(Arg.Is<BookSearchFilter>(f => f.SearchTerm == "test" && f.PageNumber == 1))
            .Returns(pagedResult);

        var result = await _sut.SearchBooksAsync(request);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    #endregion

    #region CreateBookAsync Tests

    [Fact]
    public async Task CreateBookAsync_WithValidRequest_ShouldCreateBook()
    {
        var authorId = Guid.NewGuid();
        var author = Author.Create(authorId, "Author", "a@a.com", "Bio");
        var request = new CreateBookRequest("New Book", "1234567890", "Fiction", authorId);

        _bookRepository.GetByIsbnAsync(request.Isbn).Returns((Book?)null);
        _authorRepository.GetByIdAsync(authorId).Returns(author);

        var result = await _sut.CreateBookAsync(request);

        result.Title.Should().Be(request.Title);
        await _bookRepository.Received(1).AddAsync(Arg.Any<Book>());
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task CreateBookAsync_WithDuplicateIsbn_ShouldThrowInvalidOperationException()
    {
        var request = new CreateBookRequest("New Book", "1234567890", "Fiction", Guid.NewGuid());
        _bookRepository.GetByIsbnAsync(request.Isbn).Returns(CreateDummyBook());

        var act = () => _sut.CreateBookAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>();

        await _bookRepository.DidNotReceive().AddAsync(Arg.Any<Book>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }

    #endregion

    #region UpdateBookAsync Tests

    [Fact]
    public async Task UpdateBookAsync_WithNoChanges_ShouldNotCallSaveChanges()
    {
        var bookId = Guid.NewGuid();
        var book = Book.Create(bookId, "Title", "123", Guid.NewGuid(), "Genre");
        var request = new UpdateBookRequest("Title", "123", "Genre");
        _bookRepository.GetByIdAsync(bookId).Returns(book);

        await _sut.UpdateBookAsync(bookId, request);

        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }

    #endregion

    #region DeleteBookAsync Tests

    [Fact]
    public async Task DeleteBookAsync_WithValidId_ShouldDeleteAndSave()
    {
        var bookId = Guid.NewGuid();
        var book = CreateDummyBook(bookId);
        _bookRepository.GetByIdAsync(bookId).Returns(book);

        await _sut.DeleteBookAsync(bookId);

        _bookRepository.Received(1).Delete(book);
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    #endregion
    private static Book CreateDummyBook(Guid? id = null) =>
        Book.Create(id ?? Guid.NewGuid(), "Test Book", "12345", Guid.NewGuid(), "Fiction");
}
