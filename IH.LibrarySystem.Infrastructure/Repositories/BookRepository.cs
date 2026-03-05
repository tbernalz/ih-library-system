using IH.LibrarySystem.Domain.Books;
using IH.LibrarySystem.Domain.Common;
using IH.LibrarySystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IH.LibrarySystem.Infrastructure.Repositories;

public class BookRepository(LibraryDbContext context)
    : BaseRepository<Book>(context),
        IBookRepository
{
    public async Task<Book?> GetByIsbnAsync(string isbn)
    {
        return await DbSet.FirstOrDefaultAsync(b => b.Isbn == isbn);
    }

    public async Task<bool> HasBooksByAuthorIdAsync(Guid authorId)
    {
        return await DbSet.AnyAsync(b => b.AuthorId == authorId);
    }

    public async Task<PagedResult<Book>> SearchAsync(BookSearchFilter filter)
    {
        var query = DbSet.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();

            query = query.Where(b => EF.Functions.ILike(b.Title, $"%{term}%") || b.Isbn == term);
        }

        var orderedQuery = query.OrderBy(b => b.Title).ThenBy(b => b.Id);

        return await GetPagedAsync(orderedQuery, filter.PageNumber, filter.PageSize);
    }
}
