using Bogus;
using IH.LibrarySystem.Domain.Authors;
using IH.LibrarySystem.Domain.Books;
using IH.LibrarySystem.Domain.Loans;
using IH.LibrarySystem.Domain.Members;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IH.LibrarySystem.Infrastructure.Data;

public class LibraryDataSeeder(LibraryDbContext context, ILogger<LibraryDataSeeder> logger)
{
    private const int SeedValue = 42;

    public async Task SeedAsync()
    {
        var strategy = context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                await SeedAuthorsAsync();
                await SeedMembersAsync();
                await SeedBooksAsync();
                await SeedLoansAsync();

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Seeding failed. Transaction rolled back automatically.");

                throw;
            }
        });
    }

    private async Task SeedAuthorsAsync()
    {
        if (await context.Authors.AnyAsync())
            return;

        var faker = new Faker<Author>()
            .UseSeed(SeedValue)
            .CustomInstantiator(f =>
                Author.Create(
                    Guid.NewGuid(),
                    f.Name.FullName(),
                    f.Internet.Email(),
                    f.Lorem.Sentence()
                )
            );

        var authors = faker.Generate(100);
        await context.Authors.AddRangeAsync(authors);

        await context.SaveChangesAsync();
    }

    private async Task SeedMembersAsync()
    {
        if (await context.Members.AnyAsync())
            return;

        var faker = new Faker<Member>()
            .UseSeed(SeedValue)
            .CustomInstantiator(f =>
                Member.Create(
                    Guid.NewGuid(),
                    f.Name.FullName(),
                    f.Internet.Email(),
                    f.Date.Past(1).ToUniversalTime()
                )
            );

        var members = faker.Generate(20);
        await context.Members.AddRangeAsync(members);
        await context.SaveChangesAsync();
    }

    private async Task SeedBooksAsync()
    {
        if (await context.Books.AnyAsync())
            return;

        var authorIds = await context.Authors.Select(a => a.Id).ToListAsync();
        if (authorIds.Count == 0)
            return;

        var faker = new Faker<Book>()
            .UseSeed(SeedValue)
            .CustomInstantiator(f =>
                Book.Create(
                    Guid.NewGuid(),
                    f.Commerce.ProductName(),
                    f.Commerce.Ean13(),
                    f.PickRandom(authorIds),
                    f.Music.Genre()
                )
            );

        var books = faker.Generate(50);
        await context.Books.AddRangeAsync(books);
        await context.SaveChangesAsync();
    }

    private async Task SeedLoansAsync()
    {
        if (await context.Loans.AnyAsync())
            return;

        var bookIds = await context.Books.Select(b => b.Id).ToListAsync();
        var memberIds = await context.Members.Select(m => m.Id).ToListAsync();

        if (!bookIds.Any() || !memberIds.Any())
            return;

        var faker = new Faker<Loan>()
            .UseSeed(SeedValue)
            .CustomInstantiator(f =>
            {
                var loanDate = f.Date.Recent(30).ToUniversalTime();
                return Loan.Create(
                    Guid.NewGuid(),
                    f.PickRandom(bookIds),
                    f.PickRandom(memberIds),
                    loanDate,
                    loanDate.AddDays(14).ToUniversalTime()
                );
            });

        var loans = faker.Generate(15);
        await context.Loans.AddRangeAsync(loans);
        await context.SaveChangesAsync();
    }
}
