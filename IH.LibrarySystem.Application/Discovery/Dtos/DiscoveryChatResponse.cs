using IH.LibrarySystem.Application.Books.Dtos;

namespace IH.LibrarySystem.Application.Discovery.Dtos;

public sealed record DiscoveryChatResponse(string Explanation, IReadOnlyList<BookDto> Books);
