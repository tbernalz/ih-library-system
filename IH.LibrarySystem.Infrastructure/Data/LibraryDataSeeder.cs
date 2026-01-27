using Bogus;
using IH.LibrarySystem.Domain.Authors;
using IH.LibrarySystem.Domain.Books;
using IH.LibrarySystem.Domain.Loans;
using IH.LibrarySystem.Domain.Members;
using Microsoft.EntityFrameworkCore;

namespace IH.LibrarySystem.Infrastructure.Data;

public class LibraryDataSeeder(LibraryDbContext context)
{
    public async Task SeedAsync()
    {
        if (await context.Books.AnyAsync())
            return;

        var seed = 42;

        var authorFaker = new Faker<Author>()
            .UseSeed(seed)
            .CustomInstantiator(f =>
                Author.Create(
                    Guid.NewGuid(),
                    f.Name.FullName(),
                    f.Internet.Email(),
                    f.Lorem.Sentence()
                )
            );

        var authors = authorFaker.Generate(100);

        var memberFaker = new Faker<Member>()
            .UseSeed(seed)
            .CustomInstantiator(f =>
                Member.Create(Guid.NewGuid(), f.Name.FullName(), f.Internet.Email(), f.Date.Past(1))
            );

        var members = memberFaker.Generate(20);

        var bookFaker = new Faker<Book>()
            .UseSeed(seed)
            .CustomInstantiator(f =>
                Book.Create(
                    Guid.NewGuid(),
                    f.Commerce.ProductName(),
                    f.Commerce.Ean13(),
                    f.PickRandom(authors).Id,
                    f.Music.Genre()
                )
            );

        var books = bookFaker.Generate(50);

        context.AddRange(authors);
        context.AddRange(members);
        context.AddRange(books);
        await context.SaveChangesAsync();

        var loanFaker = new Faker<Loan>()
            .UseSeed(seed)
            .CustomInstantiator(f =>
            {
                var loanDate = f.Date.Recent(30);
                return Loan.Create(
                    Guid.NewGuid(),
                    f.PickRandom(books).Id,
                    f.PickRandom(members).Id,
                    loanDate,
                    loanDate.AddDays(14)
                );
            });

        context.AddRange(loanFaker.Generate(15));
        await context.SaveChangesAsync();
    }
}
