using IH.LibrarySystem.Domain.Authors;
using IH.LibrarySystem.Domain.Books;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IH.LibrarySystem.Infrastructure.Data.Configurations;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("Books");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Title).IsRequired().HasMaxLength(200);

        builder.Property(b => b.Isbn).IsRequired().HasMaxLength(20);

        builder.Property(b => b.Genre).IsRequired().HasMaxLength(100);

        builder.Property(b => b.Status).IsRequired().HasConversion<string>().HasMaxLength(20);

        builder.Property(b => b.AuthorId).IsRequired();

        builder.Property(b => b.CreatedAt).IsRequired();

        builder.Property(b => b.UpdatedAt);

        builder
            .HasOne<Author>()
            .WithMany()
            .HasForeignKey(b => b.AuthorId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasIndex(b => b.Isbn).IsUnique();

        builder.HasIndex(b => b.AuthorId);
        builder.HasIndex(b => b.Status);
    }
}
