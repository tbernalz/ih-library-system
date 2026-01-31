using IH.LibrarySystem.Domain.Books;
using IH.LibrarySystem.Domain.Common;
using IH.LibrarySystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IH.LibrarySystem.Infrastructure.Repositories;

public class BookRepository : BaseRepository<Book>, IBookRepository
{
    public BookRepository(LibraryDbContext context)
        : base(context) { }

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
        var query = DbSet.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            query = query.Where(b =>
                b.Title.Contains(filter.SearchTerm) || b.Isbn.Contains(filter.SearchTerm)
            );
        }

        query = query.OrderBy(b => b.Title);

        return await GetPagedAsync(query, filter.PageNumber, filter.PageSize);
    }
}
