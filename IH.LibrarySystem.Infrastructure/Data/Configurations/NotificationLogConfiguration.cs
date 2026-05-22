using IH.LibrarySystem.Domain.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IH.LibrarySystem.Infrastructure.Data.Configurations;

public class NotificationLogConfiguration : IEntityTypeConfiguration<NotificationLog>
{
    public void Configure(EntityTypeBuilder<NotificationLog> builder)
    {
        builder.HasKey(n => n.Id);

        builder.Property(n => n.LoanId).IsRequired();

        builder.Property(n => n.Type).IsRequired().HasConversion<string>().HasMaxLength(50);

        builder.Property(n => n.SentAt).IsRequired();

        builder.HasIndex(n => new { n.LoanId, n.Type }).IsUnique();

        builder.HasIndex(n => n.LoanId);
    }
}
