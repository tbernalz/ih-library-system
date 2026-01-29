namespace IH.LibrarySystem.Domain.Books;

public record BookSearchFilter(string? SearchTerm, int PageNumber, int PageSize);
