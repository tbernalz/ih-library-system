using IH.LibrarySystem.Domain.Authors;
using IH.LibrarySystem.Domain.Books;
using IH.LibrarySystem.Domain.Identity;
using IH.LibrarySystem.Domain.Loans;
using IH.LibrarySystem.Domain.Members;
using IH.LibrarySystem.Domain.Notifications;
using Microsoft.EntityFrameworkCore;

namespace IH.LibrarySystem.Infrastructure.Data;

public class LibraryDbContext : DbContext
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options)
        : base(options) { }

    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("vector");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LibraryDbContext).Assembly);
    }
}
