using IH.LibrarySystem.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IH.LibrarySystem.Infrastructure.Data.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.UserId).IsRequired();

        builder.Property(t => t.TokenHash).IsRequired().HasMaxLength(64);

        builder.Property(t => t.ExpiresAt).IsRequired();

        builder.Property(t => t.RevokedAt);

        builder.Property(t => t.ReplacedByTokenHash).HasMaxLength(64);

        builder.Property(t => t.CreatedByIp).HasMaxLength(45);

        builder.Property(t => t.RevokedByIp).HasMaxLength(45);

        builder.Property(t => t.CreatedAt).IsRequired();

        builder.Property(t => t.UpdatedAt);

        builder
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.HasIndex(t => t.TokenHash).IsUnique();
        builder.HasIndex(t => t.UserId);
    }
}
