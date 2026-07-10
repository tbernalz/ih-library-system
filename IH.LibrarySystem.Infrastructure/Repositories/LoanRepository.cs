using IH.LibrarySystem.Domain.Common;
using IH.LibrarySystem.Domain.Loans;
using IH.LibrarySystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IH.LibrarySystem.Infrastructure.Repositories;

public class LoanRepository(LibraryDbContext context)
    : BaseRepository<Loan>(context),
        ILoanRepository
{
    public async Task<Loan?> GetWithBookAsync(
        Guid loanId,
        bool readOnly = false,
        CancellationToken cancellationToken = default
    )
    {
        var query = Context.Loans.Include(l => l.Book).AsQueryable();
        if (readOnly)
        {
            query = query.AsNoTracking();
        }
        return await query.FirstOrDefaultAsync(l => l.Id == loanId, cancellationToken);
    }

    public async Task<PagedResult<Loan>> SearchAsync(
        LoanSearchFilter filter,
        bool readOnly = false,
        CancellationToken cancellationToken = default
    )
    {
        var query = DbSet.AsQueryable();
        if (readOnly)
        {
            query = query.AsNoTracking();
        }

        if (filter.MemberId.HasValue)
            query = query.Where(l => l.MemberId == filter.MemberId);

        if (filter.BookId.HasValue)
            query = query.Where(l => l.BookId == filter.BookId);

        if (filter.IsActive.HasValue)
            query = query.Where(l => (l.ReturnDate == null) == filter.IsActive);

        if (filter.IsOverdue == true)
            query = query.Where(l => l.ReturnDate == null && l.DueDate < DateTime.UtcNow);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(l => l.LoanDate)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Include(l => l.Book)
            .ToListAsync(cancellationToken);

        return new PagedResult<Loan>(items, totalCount, filter.PageNumber, filter.PageSize);
    }
}
