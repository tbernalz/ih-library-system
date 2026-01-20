using IH.LibrarySystem.Domain.SharedKernel;
using IH.LibrarySystem.Infrastructure.Data;

namespace IH.LibrarySystem.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly LibraryDbContext _context;

    public UnitOfWork(LibraryDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
