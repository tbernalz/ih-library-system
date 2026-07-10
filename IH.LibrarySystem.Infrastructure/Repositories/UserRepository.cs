using IH.LibrarySystem.Domain.Identity;
using IH.LibrarySystem.Infrastructure.Data;
using IH.LibrarySystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IH.LibrarySystem.Infrastructure.Repositories;

public sealed class UserRepository(LibraryDbContext context)
    : BaseRepository<User>(context),
        IUserRepository
{
    public async Task<User?> GetByGoogleSubjectIdAsync(
        string googleSubjectId,
        bool readOnly = false,
        CancellationToken cancellationToken = default
    )
    {
        var query = DbSet.AsQueryable();
        if (readOnly)
        {
            query = query.AsNoTracking();
        }
        return await query.FirstOrDefaultAsync(
            u => u.GoogleSubjectId == googleSubjectId,
            cancellationToken
        );
    }
}
