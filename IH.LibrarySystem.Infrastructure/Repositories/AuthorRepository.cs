using IH.LibrarySystem.Domain.Authors;
using IH.LibrarySystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IH.LibrarySystem.Infrastructure.Repositories;

public class AuthorRepository(LibraryDbContext context)
    : BaseRepository<Author>(context),
        IAuthorRepository
{
    public async Task<Author?> GetByEmailAsync(string email)
    {
        return await DbSet.AsNoTracking().FirstOrDefaultAsync(a => a.Email == email);
    }
}
