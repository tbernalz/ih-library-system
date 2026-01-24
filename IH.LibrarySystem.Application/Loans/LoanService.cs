using IH.LibrarySystem.Application.Loans.Dtos;
using IH.LibrarySystem.Domain.Loans;
using Microsoft.Extensions.Logging;

namespace IH.LibrarySystem.Application.Loans;

public class LoanService(ILoanRepository loanRepository, ILogger<LoanService> logger) : ILoanService
{
    private const int DefaultLoanDurationDays = 14;

    public async Task<LoanDto> GetLoanByIdAsync(Guid loanId)
    {
        logger.LogDebug("Fetching loan with {LoanId}", loanId);

        var loan = await loanRepository.GetByIdAsync(loanId);

        if (loan is null)
        {
            logger.LogWarning("Loan retrieval failed: ID {LoanId} not found", loanId);
            throw new KeyNotFoundException($"Loan with ID {loanId} not found.");
        }

        return MapToDto(loan);
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
