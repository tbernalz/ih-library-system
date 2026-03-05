using IH.LibrarySystem.Domain.Common;
using IH.LibrarySystem.Domain.Loans;
using IH.LibrarySystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IH.LibrarySystem.Infrastructure.Repositories;

public class LoanRepository(LibraryDbContext context)
    : BaseRepository<Loan>(context),
        ILoanRepository
{
    public async Task<Loan?> GetWithBookAsync(Guid loanId)
    {
        return await Context.Loans.Include(l => l.Book).FirstOrDefaultAsync(l => l.Id == loanId);
    }

    public async Task<PagedResult<Loan>> SearchAsync(LoanSearchFilter filter)
    {
        var query = DbSet.AsNoTracking();

        if (filter.MemberId.HasValue)
            query = query.Where(l => l.MemberId == filter.MemberId);

        if (filter.BookId.HasValue)
            query = query.Where(l => l.BookId == filter.BookId);

        if (filter.IsActive.HasValue)
            query = query.Where(l => (l.ReturnDate == null) == filter.IsActive);

        if (filter.IsOverdue == true)
            query = query.Where(l => l.ReturnDate == null && l.DueDate < DateTime.UtcNow);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(l => l.LoanDate)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Include(l => l.Book)
            .ToListAsync();

        return new PagedResult<Loan>(items, totalCount, filter.PageNumber, filter.PageSize);
    }
}
