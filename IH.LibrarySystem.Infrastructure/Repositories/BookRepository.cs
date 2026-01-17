using IH.LibrarySystem.Domain.Books;
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
}
