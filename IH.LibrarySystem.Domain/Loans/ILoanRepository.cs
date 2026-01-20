namespace IH.LibrarySystem.Domain.Loans;

public interface ILoanRepository
{
    Task<Loan?> GetByIdAsync(Guid id);

    Task AddAsync(Loan loan);
    void Delete(Loan loan);
}
