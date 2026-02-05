using IH.LibrarySystem.Application.Loans.Dtos;

namespace IH.LibrarySystem.Application.Loans;

public interface ILoanService
{
    Task<LoanDto> GetLoanByIdAsync(Guid loanId);

    Task<LoanDto> CheckoutBookAsync(CheckoutBookRequest request);

    Task<LoanDto> ReturnBookAsync(Guid loanId, ReturnBookRequest request);
}
