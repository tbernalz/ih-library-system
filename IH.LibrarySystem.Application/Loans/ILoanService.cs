using IH.LibrarySystem.Application.Loans.Dtos;
using IH.LibrarySystem.Domain.Common;
using IH.LibrarySystem.Domain.Loans;

namespace IH.LibrarySystem.Application.Loans;

public interface ILoanService
{
    Task<PagedResult<LoanDto>> GetLoansAsync(LoanSearchFilter filter);
    Task<LoanDto> GetLoanByIdAsync(Guid loanId);

    Task<LoanDto> CheckoutBookAsync(CheckoutBookRequest request);

    Task<LoanDto> ReturnBookAsync(Guid loanId, ReturnBookRequest request);
}
