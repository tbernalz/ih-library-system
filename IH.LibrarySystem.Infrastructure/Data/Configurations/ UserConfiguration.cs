using IH.LibrarySystem.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IH.LibrarySystem.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.GoogleSubjectId).IsRequired().HasMaxLength(255);

        builder.Property(u => u.Email).IsRequired().HasMaxLength(255);

        builder.Property(u => u.DisplayName).IsRequired().HasMaxLength(200);

        builder.Property(u => u.AvatarUrl).HasMaxLength(2048);

        builder.Property(u => u.Role).IsRequired().HasConversion<string>().HasMaxLength(20);

        builder.Property(u => u.LastLoginAt).IsRequired();

        builder.Property(u => u.IsDisabled).IsRequired();

        builder.Property(u => u.CreatedAt).IsRequired();

        builder.Property(u => u.UpdatedAt);

        builder.HasIndex(u => u.GoogleSubjectId).IsUnique();
        builder.HasIndex(u => u.Email).IsUnique();
    }
}
