using IH.LibrarySystem.Application.Books.Dtos;
using IH.LibrarySystem.Domain.Authors;
using IH.LibrarySystem.Domain.Books;

namespace IH.LibrarySystem.Application.Books;

public class BookService(IBookRepository bookRepository, IAuthorRepository authorRepository)
    : IBookService
{
    public async Task<BookDto?> GetBookByIdAsync(Guid bookId)
    {
        var book = await bookRepository.GetByIdAsync(bookId);
        return book is null ? null : MapToDto(book);
    }

    public async Task<BookDto> CreateBookAsync(CreateBookRequestDto request)
    {
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
        await bookRepository.SaveChangesAsync();

        return MapToDto(book);
    }

    public async Task<BookDto> UpdateBookAsync(Guid bookId, UpdateBookRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var book =
            await bookRepository.GetByIdAsync(bookId)
            ?? throw new KeyNotFoundException($"Book with ID {bookId} not found.");

        book.ChangeMetadata(request.Title, request.Isbn, request.Genre);
        bookRepository.Update(book);
        await bookRepository.SaveChangesAsync();

        return MapToDto(book);
    }

    public async Task DeleteBookAsync(Guid bookId)
    {
        var book =
            await bookRepository.GetByIdAsync(bookId)
            ?? throw new KeyNotFoundException($"Book with ID {bookId} not found.");

        if (book is null)
            return;

        bookRepository.Delete(book);
        await bookRepository.SaveChangesAsync();
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
