using IH.LibrarySystem.Domain.Common;
using IH.LibrarySystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IH.LibrarySystem.Infrastructure.Repositories;

public abstract class BaseRepository<T>
    where T : class
{
    protected readonly LibraryDbContext Context;
    protected readonly DbSet<T> DbSet;

    protected BaseRepository(LibraryDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id) => await DbSet.FindAsync(id);

    protected async Task<PagedResult<T>> GetPagedAsync(
        IQueryable<T> query,
        int pageNumber,
        int pageSize
    )
    {
        var totalCount = await query.CountAsync();
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<T>(items, totalCount, pageNumber, pageSize);
    }

    public virtual async Task AddAsync(T entity) => await DbSet.AddAsync(entity);

    public virtual void Delete(T entity) => DbSet.Remove(entity);
}
