using IH.LibrarySystem.Domain.Books;
using IH.LibrarySystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IH.LibrarySystem.Infrastructure.Repositories;

public class BookRepository : BaseRepository<Book>, IBookRepository
{
    public BookRepository(LibraryDbContext context)
        : base(context) { }

    public async Task<IReadOnlyList<Book>> SearchAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return Array.Empty<Book>();

        var lowerSearchTerm = searchTerm.ToLower();

        return await DbSet
            .Where(b =>
                b.Title.ToLower().Contains(lowerSearchTerm)
                || b.Isbn.ToLower().Contains(lowerSearchTerm)
                || b.Genre.ToLower().Contains(lowerSearchTerm)
            )
            .ToListAsync();
    }
}
