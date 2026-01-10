using IH.LibrarySystem.Domain.Members;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IH.LibrarySystem.Infrastructure.Data.Configurations;

public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.ToTable("Members");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name).IsRequired().HasMaxLength(200);

        builder.Property(m => m.Email).IsRequired().HasMaxLength(255);

        builder.Property(m => m.JoinDate).IsRequired();

        builder.Property(m => m.Status).IsRequired().HasConversion<int>();

        builder.Property(m => m.CreatedAt).IsRequired();

        builder.Property(m => m.UpdatedAt);

        builder.HasIndex(m => m.Email).IsUnique();

        builder.HasIndex(m => m.Status);
    }
}
