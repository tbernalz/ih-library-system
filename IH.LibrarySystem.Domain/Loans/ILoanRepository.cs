namespace IH.LibrarySystem.Domain.Loans;

public interface ILoanRepository
{
    Task<Loan?> GetByIdAsync(Guid id);

    Task AddAsync(Loan loan);
    void Update(Loan loan);
    void Delete(Loan loan);

    Task SaveChangesAsync();
}
