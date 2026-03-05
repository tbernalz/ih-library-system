using IH.LibrarySystem.Domain.Members;
using IH.LibrarySystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IH.LibrarySystem.Infrastructure.Repositories;

public class MemberRepository(LibraryDbContext context)
    : BaseRepository<Member>(context),
        IMemberRepository
{
    public async Task<Member?> GetByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        return await DbSet.FirstOrDefaultAsync(m => EF.Functions.ILike(m.Email, email));
    }
}
