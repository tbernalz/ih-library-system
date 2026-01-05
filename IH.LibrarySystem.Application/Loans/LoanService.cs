using IH.LibrarySystem.Application.Loans.Dtos;
using IH.LibrarySystem.Domain.Loans;

namespace IH.LibrarySystem.Application.Loans;

public class LoanService(ILoanRepository loanRepository) : ILoanService
{
    private const int DefaultLoanDurationDays = 14;

    public async Task<LoanDto?> GetLoanByIdAsync(Guid loanId)
    {
        var loan = await loanRepository.GetByIdAsync(loanId);
        return loan is not null ? MapToDto(loan) : null;
    }

    private static LoanDto MapToDto(Loan loan) =>
        new()
        {
            Id = loan.Id,
            BookId = loan.BookId,
            MemberId = loan.MemberId,
            LoanDate = loan.LoanDate,
            DueDate = loan.DueDate,
            ReturnDate = loan.ReturnDate,
            FineAmount = loan.FineAmount,
        };
}
