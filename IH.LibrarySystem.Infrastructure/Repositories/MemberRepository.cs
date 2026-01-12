using IH.LibrarySystem.Domain.Members;
using IH.LibrarySystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IH.LibrarySystem.Infrastructure.Repositories;

public class MemberRepository : BaseRepository<Member>, IMemberRepository
{
    public MemberRepository(LibraryDbContext context)
        : base(context) { }

    public async Task<Member?> GetByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var lowerEmail = email.ToLower();
        return await DbSet.FirstOrDefaultAsync(m => m.Email.ToLower() == lowerEmail);
    }
}
