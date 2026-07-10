using IH.LibrarySystem.Domain.Common;

namespace IH.LibrarySystem.Domain.Loans;

public interface ILoanRepository
{
    Task<Loan?> GetByIdAsync(
        Guid id,
        bool readOnly = false,
        CancellationToken cancellationToken = default
    );
    Task<Loan?> GetWithBookAsync(
        Guid loanId,
        bool readOnly = false,
        CancellationToken cancellationToken = default
    );
    Task<PagedResult<Loan>> SearchAsync(
        LoanSearchFilter filter,
        bool readOnly = false,
        CancellationToken cancellationToken = default
    );

    Task AddAsync(Loan loan);
    void Delete(Loan loan);
}
