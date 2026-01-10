using IH.LibrarySystem.Domain.Authors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IH.LibrarySystem.Infrastructure.Data.Configurations;

public class AuthorConfiguration : IEntityTypeConfiguration<Author>
{
    public void Configure(EntityTypeBuilder<Author> builder)
    {
        builder.ToTable("Authors");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name).IsRequired().HasMaxLength(200);

        builder.Property(a => a.Email).IsRequired().HasMaxLength(255);

        builder.Property(a => a.Bio).HasMaxLength(1000);

        builder.Property(a => a.CreatedAt).IsRequired();

        builder.Property(a => a.UpdatedAt);

        builder.HasIndex(a => a.Email).IsUnique();
    }
}
