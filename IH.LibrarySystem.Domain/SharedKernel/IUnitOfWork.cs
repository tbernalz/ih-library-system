namespace IH.LibrarySystem.Domain.SharedKernel;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync();
}
