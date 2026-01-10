using IH.LibrarySystem.Domain.Books;
using IH.LibrarySystem.Domain.Loans;
using IH.LibrarySystem.Domain.Members;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IH.LibrarySystem.Infrastructure.Data.Configurations;

public class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.ToTable("Loans");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.BookId).IsRequired();

        builder.Property(l => l.MemberId).IsRequired();

        builder.Property(l => l.LoanDate).IsRequired();

        builder.Property(l => l.DueDate).IsRequired();

        builder.Property(l => l.ReturnDate);

        builder.Property(l => l.FineAmount).HasPrecision(18, 2);

        builder.Property(l => l.CreatedAt).IsRequired();

        builder.Property(l => l.UpdatedAt);

        builder
            .HasOne<Book>()
            .WithMany()
            .HasForeignKey(l => l.BookId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder
            .HasOne<Member>()
            .WithMany()
            .HasForeignKey(l => l.MemberId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasIndex(l => l.BookId);
        builder.HasIndex(l => l.MemberId);
        builder.HasIndex(l => l.LoanDate);
        builder.HasIndex(l => l.DueDate);
        builder.HasIndex(l => new { l.BookId, l.ReturnDate });
    }
}
