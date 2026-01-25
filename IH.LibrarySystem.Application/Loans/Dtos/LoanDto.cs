using IH.LibrarySystem.Application.Books.Dtos;
using IH.LibrarySystem.Application.Members.Dtos;

namespace IH.LibrarySystem.Application.Loans.Dtos;

public record LoanDto
{
    public required Guid Id { get; init; }
    public required Guid BookId { get; init; }
    public required Guid MemberId { get; init; }
    public DateTime LoanDate { get; init; }
    public DateTime DueDate { get; init; }
    public DateTime? ReturnDate { get; init; }
    public decimal? FineAmount { get; init; }
}
