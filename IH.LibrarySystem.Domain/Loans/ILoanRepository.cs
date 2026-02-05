namespace IH.LibrarySystem.Domain.Loans;

public interface ILoanRepository
{
    Task<Loan?> GetByIdAsync(Guid id);
    Task<Loan?> GetWithBookAsync(Guid loanId);

    Task AddAsync(Loan loan);
    void Delete(Loan loan);
}
