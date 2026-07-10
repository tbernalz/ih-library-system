using IH.LibrarySystem.Domain.Books;
using IH.LibrarySystem.Domain.Common;
using IH.LibrarySystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IH.LibrarySystem.Infrastructure.Repositories;

public class BookRepository(LibraryDbContext context)
    : BaseRepository<Book>(context),
        IBookRepository
{
    public async Task<Book?> GetByIsbnAsync(
        string isbn,
        bool readOnly = false,
        CancellationToken cancellationToken = default
    )
    {
        var query = DbSet.AsQueryable();
        if (readOnly)
        {
            query = query.AsNoTracking();
        }
        return await query.FirstOrDefaultAsync(b => b.Isbn == isbn, cancellationToken);
    }

    public async Task<bool> HasBooksByAuthorIdAsync(
        Guid authorId,
        bool readOnly = false,
        CancellationToken cancellationToken = default
    )
    {
        var query = DbSet.AsQueryable();
        if (readOnly)
        {
            query = query.AsNoTracking();
        }
        return await query.AnyAsync(b => b.AuthorId == authorId, cancellationToken);
    }

    public async Task<PagedResult<Book>> SearchAsync(
        BookSearchFilter filter,
        bool readOnly = false,
        CancellationToken cancellationToken = default
    )
    {
        var query = DbSet.AsQueryable();
        if (readOnly)
        {
            query = query.AsNoTracking();
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();

            query = query.Where(b => b.Title.Contains(term) || b.Isbn == term);
        }

        var orderedQuery = query.OrderBy(b => b.Title).ThenBy(b => b.Id);

        return await GetPagedAsync(orderedQuery, filter.PageNumber, filter.PageSize);
    }
}
