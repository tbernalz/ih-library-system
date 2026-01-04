using System.ComponentModel.DataAnnotations;

namespace IH.LibrarySystem.Application.Loans.Dtos;

public record CheckoutBookRequest([Required] Guid BookId, [Required] Guid MemberId);

public record ReturnBookRequest([Required] Guid LoanId, DateTime? ReturnDate = null);

public record CalculateFineRequest([Required] Guid LoanId, [Range(0.01, 100.00)] decimal DailyRate);
