using IH.LibrarySystem.Domain.Common;
using IH.LibrarySystem.Domain.Loans;
using IH.LibrarySystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IH.LibrarySystem.Infrastructure.Repositories;

public class LoanRepository : BaseRepository<Loan>, ILoanRepository
{
    public LoanRepository(LibraryDbContext context)
        : base(context) { }

    public async Task<Loan?> GetWithBookAsync(Guid loanId)
    {
        return await Context.Loans.Include(l => l.Book).FirstOrDefaultAsync(l => l.Id == loanId);
    }

    public async Task<Loan?> GetActiveLoanByBookIdAsync(Guid bookId)
    {
        return await DbSet.FirstOrDefaultAsync(l => l.BookId == bookId && l.ReturnDate == null);
    }

    public async Task<IReadOnlyList<Loan>> GetByMemberAsync(Guid memberId)
    {
        return await DbSet
            .Where(l => l.MemberId == memberId)
            .OrderByDescending(l => l.LoanDate)
            .ToListAsync();
    }

    public async Task<PagedResult<Loan>> SearchAsync(LoanSearchFilter filter)
    {
        var query = DbSet.AsNoTracking();

        if (filter.MemberId.HasValue)
            query = query.Where(l => l.MemberId == filter.MemberId.Value);

        if (filter.BookId.HasValue)
            query = query.Where(l => l.BookId == filter.BookId.Value);

        if (filter.IsActive.HasValue)
            query = query.Where(l => (l.ReturnDate == null) == filter.IsActive.Value);

        if (filter.IsOverdue == true)
            query = query.Where(l => l.ReturnDate == null && l.DueDate < DateTime.UtcNow);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(l => l.LoanDate)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<Loan>(items, totalCount, filter.PageNumber, filter.PageSize);
    }
}
