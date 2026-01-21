using IH.LibrarySystem.Application.Books.Dtos;
using IH.LibrarySystem.Domain.Authors;
using IH.LibrarySystem.Domain.Books;
using IH.LibrarySystem.Domain.SharedKernel;

namespace IH.LibrarySystem.Application.Books;

public class BookService(
    IBookRepository bookRepository,
    IAuthorRepository authorRepository,
    IUnitOfWork unitOfWork
) : IBookService
{
    public async Task<BookDto> GetBookByIdAsync(Guid bookId)
    {
        var book =
            await bookRepository.GetByIdAsync(bookId)
            ?? throw new KeyNotFoundException($"Book with ID {bookId} not found.");

        return MapToDto(book);
    }

    public async Task<BookDto> CreateBookAsync(CreateBookRequest request)
    {
        var existingBook = await bookRepository.GetByIsbnAsync(request.Isbn);

        if (existingBook != null)
        {
            throw new InvalidOperationException($"Book with ISBN {request.Isbn} already exists.");
        }

        var author =
            await authorRepository.GetByIdAsync(request.AuthorId)
            ?? throw new KeyNotFoundException($"Author with ID {request.AuthorId} not found.");

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

        var book =
            await bookRepository.GetByIdAsync(bookId)
            ?? throw new KeyNotFoundException($"Book with ID {bookId} not found.");

        if (request.Title == book.Title && request.Isbn == book.Isbn && request.Genre == book.Genre)
            return MapToDto(book);

        var existingBook = await bookRepository.GetByIsbnAsync(request.Isbn);

        if (existingBook is not null && existingBook.Id != book.Id)
            throw new InvalidOperationException(
                $"ISBN '{request.Isbn}' is already taken by another book."
            );

        book.ChangeMetadata(request.Title, request.Isbn, request.Genre);
        await unitOfWork.SaveChangesAsync();

        return MapToDto(book);
    }

    public async Task DeleteBookAsync(Guid bookId)
    {
        var book =
            await bookRepository.GetByIdAsync(bookId)
            ?? throw new KeyNotFoundException($"Book with ID {bookId} not found.");

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
            Author = null,
        };
}
