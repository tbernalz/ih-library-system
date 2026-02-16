using IH.LibrarySystem.Domain.Common;

namespace IH.LibrarySystem.Domain.Books;

public record BookSearchFilter(string? SearchTerm) : PagedFilter();
