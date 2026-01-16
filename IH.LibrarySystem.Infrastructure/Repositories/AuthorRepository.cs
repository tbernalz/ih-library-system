using IH.LibrarySystem.Domain.Authors;
using IH.LibrarySystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IH.LibrarySystem.Infrastructure.Repositories;

public class AuthorRepository : BaseRepository<Author>, IAuthorRepository
{
    public AuthorRepository(LibraryDbContext context)
        : base(context) { }

    public async Task<Author?> GetByEmailAsync(string email)
    {
        return await DbSet.FirstOrDefaultAsync(a => a.Email == email);
    }
}
