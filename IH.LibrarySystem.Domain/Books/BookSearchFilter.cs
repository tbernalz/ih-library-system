using IH.LibrarySystem.Domain.Common;

namespace IH.LibrarySystem.Domain.Books;

public record BookSearchFilter(string? SearchTerm, int PageNumber = 1, int PageSize = 10)
    : PagedFilter(PageNumber, PageSize);
