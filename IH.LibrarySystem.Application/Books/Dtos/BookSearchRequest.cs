namespace IH.LibrarySystem.Application.Books.Dtos;

public record BookSearchRequest(string? SearchTerm, int PageNumber = 1, int PageSize = 10);
