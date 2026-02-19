using IH.LibrarySystem.Application.Books.Dtos;
using IH.LibrarySystem.Domain.Authors;
using IH.LibrarySystem.Domain.Books;
using IH.LibrarySystem.Domain.Common;
using IH.LibrarySystem.Domain.SharedKernel;
using Microsoft.Extensions.Logging;

namespace IH.LibrarySystem.Application.Books;

public class BookService(
    IBookRepository bookRepository,
    IAuthorRepository authorRepository,
    IUnitOfWork unitOfWork,
    ILogger<BookService> logger
) : IBookService
{
    public async Task<BookDto> GetBookByIdAsync(Guid bookId)
    {
        logger.LogDebug("Fetching book {BookId}", bookId);

        var book = await bookRepository.GetByIdAsync(bookId);
        if (book is null)
        {
            logger.LogWarning("Book retrieval failed: ID {BookId} not found", bookId);
            throw new KeyNotFoundException($"Book with ID {bookId} not found.");
        }

        return MapToDto(book);
    }

    public async Task<PagedResult<BookDto>> SearchBooksAsync(BookSearchRequest request)
    {
        logger.LogDebug("Searching books with term: {SearchTerm}", request.SearchTerm);

        var filter = new BookSearchFilter(request.SearchTerm, request.PageNumber, request.PageSize);
        var result = await bookRepository.SearchAsync(filter);

        return new PagedResult<BookDto>(
            result.Items.Select(MapToDto).ToList(),
            result.TotalCount,
            result.PageNumber,
            result.PageSize
        );
    }

    public async Task<BookDto> CreateBookAsync(CreateBookRequest request)
    {
        logger.LogInformation("Initiating book creation: request {request}", request);

        var existingBook = await bookRepository.GetByIsbnAsync(request.Isbn);

        if (existingBook is not null)
        {
            logger.LogWarning("CreateBook rejected: Duplicate ISBN {Isbn}", request.Isbn);
            throw new InvalidOperationException($"Book with ISBN {request.Isbn} already exists.");
        }

        var author = await authorRepository.GetByIdAsync(request.AuthorId);
        if (author is null)
        {
            logger.LogWarning("CreateBook failed: Author {AuthorId} not found", request.AuthorId);
            throw new KeyNotFoundException($"Author with ID {request.AuthorId} not found.");
        }

        var book = Book.Create(
            Guid.NewGuid(),
            request.Title,
            request.Isbn,
            request.AuthorId,
            request.Genre
        );
        book.AssignAuthor(author.Id);

        await bookRepository.AddAsync(book);
        await unitOfWork.SaveChangesAsync();

        return MapToDto(book);
    }

    public async Task<BookDto> UpdateBookAsync(Guid bookId, UpdateBookRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        logger.LogInformation("Initiating update for book: {BookId}", bookId);

        var book = await bookRepository.GetByIdAsync(bookId);
        if (book is null)
        {
            logger.LogWarning("UpdateBook failed: Book {BookId} not found", bookId);
            throw new KeyNotFoundException($"Book with ID {bookId} not found.");
        }

        if (request.Title == book.Title && request.Isbn == book.Isbn && request.Genre == book.Genre)
        {
            logger.LogInformation("UpdateBook: No changes detected for {BookId}", bookId);
            return MapToDto(book);
        }

        var existingBook = await bookRepository.GetByIsbnAsync(request.Isbn);

        if (existingBook is not null && existingBook.Id != book.Id)
        {
            logger.LogWarning(
                "UpdateBook failed: ISBN {Isbn} already used by {OtherBookId}",
                request.Isbn,
                existingBook.Id
            );
            throw new InvalidOperationException($"ISBN '{request.Isbn}' is taken.");
        }

        book.ChangeMetadata(request.Title, request.Isbn, request.Genre);
        await unitOfWork.SaveChangesAsync();

        return MapToDto(book);
    }

    public async Task DeleteBookAsync(Guid bookId)
    {
        logger.LogInformation("Initiating book deletion: {BookId}", bookId);

        var book = await bookRepository.GetByIdAsync(bookId);
        if (book is null)
        {
            logger.LogWarning("DeleteBook failed: Book {BookId} not found", bookId);
            throw new KeyNotFoundException($"Book with ID {bookId} not found.");
        }

        bookRepository.Delete(book);
        await unitOfWork.SaveChangesAsync();
    }

    private static BookDto MapToDto(Book book) =>
        new()
        {
            Id = book.Id,
            Title = book.Title,
            Isbn = book.Isbn,
            Status = book.Status,
            AuthorId = book.AuthorId,
        };
}
