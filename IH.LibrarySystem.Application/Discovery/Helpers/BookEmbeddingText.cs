using IH.LibrarySystem.Domain.Books;

namespace IH.LibrarySystem.Application.Discovery.Helpers;

internal static class BookEmbeddingText
{
    public static string Format(Book book) =>
        $"Title: {book.Title}. Genre: {book.Genre}. ISBN: {book.Isbn}.";
}
