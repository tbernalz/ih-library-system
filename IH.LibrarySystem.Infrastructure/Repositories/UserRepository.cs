using IH.LibrarySystem.Domain.Identity;
using IH.LibrarySystem.Infrastructure.Data;
using IH.LibrarySystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IH.LibrarySystem.Infrastructure.Repositories;

public sealed class UserRepository(LibraryDbContext context)
    : BaseRepository<User>(context),
        IUserRepository
{
    public async Task<User?> GetByGoogleSubjectIdAsync(string googleSubjectId) =>
        await DbSet.FirstOrDefaultAsync(u => u.GoogleSubjectId == googleSubjectId);

    public async Task<User?> GetByEmailAsync(string email) =>
        await DbSet.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
}
