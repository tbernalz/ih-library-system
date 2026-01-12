using IH.LibrarySystem.Domain.Loans;
using IH.LibrarySystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IH.LibrarySystem.Infrastructure.Repositories;

public class LoanRepository : BaseRepository<Loan>, ILoanRepository
{
    public LoanRepository(LibraryDbContext context)
        : base(context) { }

    public async Task<Loan?> GetActiveLoanByBookIdAsync(Guid bookId)
    {
        return await DbSet.FirstOrDefaultAsync(l => l.BookId == bookId && l.ReturnDate == null);
    }

    public async Task<IReadOnlyList<Loan>> GetMemberLoanHistoryAsync(Guid memberId)
    {
        return await DbSet
            .Where(l => l.MemberId == memberId)
            .OrderByDescending(l => l.LoanDate)
            .ToListAsync();
    }
}
