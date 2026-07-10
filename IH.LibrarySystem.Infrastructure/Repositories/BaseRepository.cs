using IH.LibrarySystem.Domain.Common;
using IH.LibrarySystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IH.LibrarySystem.Infrastructure.Repositories;

public abstract class BaseRepository<T>(LibraryDbContext context)
    where T : class
{
    protected readonly LibraryDbContext Context = context;
    protected readonly DbSet<T> DbSet = context.Set<T>();

    public virtual async Task<T?> GetByIdAsync(
        Guid id,
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
            e => EF.Property<Guid>(e, "Id") == id,
            cancellationToken
        );
    }

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
