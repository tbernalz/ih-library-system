using IH.LibrarySystem.Domain.Members;
using IH.LibrarySystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IH.LibrarySystem.Infrastructure.Repositories;

public class MemberRepository(LibraryDbContext context)
    : BaseRepository<Member>(context),
        IMemberRepository
{
    public async Task<Member?> GetByEmailAsync(
        string email,
        bool readOnly = false,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var query = DbSet.AsQueryable();
        if (readOnly)
        {
            query = query.AsNoTracking();
        }
        return await query.FirstOrDefaultAsync(m => m.Email == email, cancellationToken);
    }
}
