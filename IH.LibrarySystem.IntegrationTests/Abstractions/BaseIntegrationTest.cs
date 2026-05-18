using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using IH.LibrarySystem.Domain.Authors;
using IH.LibrarySystem.Domain.Books;
using IH.LibrarySystem.Domain.Loans;
using IH.LibrarySystem.Domain.Members;
using IH.LibrarySystem.Infrastructure.Data;
using IH.LibrarySystem.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Pgvector;

namespace IH.LibrarySystem.IntegrationTests.Abstractions;

public abstract class BaseIntegrationTest : IAsyncLifetime
{
    protected static readonly JsonSerializerOptions SerializerOptions = new(
        JsonSerializerDefaults.Web
    )
    {
        Converters = { new JsonStringEnumConverter() },
    };

    protected IntegrationTestFixture Fixture { get; }

    protected HttpClient Client => Fixture.Client;

    protected BaseIntegrationTest(IntegrationTestFixture fixture) => Fixture = fixture;

    public Task InitializeAsync() => Fixture.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    protected async Task<Guid> PersistAuthorAsync(string name, string email, string? bio = null)
    {
        using var scope = Fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        var author = Author.Create(Guid.NewGuid(), name, email, bio);
        db.Authors.Add(author);
        await db.SaveChangesAsync().ConfigureAwait(false);
        return author.Id;
    }

    protected async Task<Guid> PersistMemberAsync(string name, string email)
    {
        using var scope = Fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        var member = Member.Create(Guid.NewGuid(), name, email);
        db.Members.Add(member);
        await db.SaveChangesAsync().ConfigureAwait(false);
        return member.Id;
    }

    protected async Task<Guid> PersistBookAsync(
        string title,
        string isbn,
        string genre,
        Guid authorId
    )
    {
        using var scope = Fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        var book = Book.Create(Guid.NewGuid(), title, isbn, authorId, genre);
        db.Books.Add(book);
        await db.SaveChangesAsync().ConfigureAwait(false);
        return book.Id;
    }

    protected async Task<Member?> GetMemberEntityAsync(Guid id)
    {
        using var scope = Fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        return await db
            .Members.AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id)
            .ConfigureAwait(false);
    }

    protected async Task<Book?> GetBookEntityAsync(Guid id)
    {
        using var scope = Fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        return await db
            .Books.AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id)
            .ConfigureAwait(false);
    }

    protected async Task<Author?> GetAuthorEntityAsync(Guid id)
    {
        using var scope = Fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        return await db
            .Authors.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id)
            .ConfigureAwait(false);
    }

    protected async Task<bool> AuthorExistsAsync(Guid id)
    {
        using var scope = Fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        return await db.Authors.AnyAsync(a => a.Id == id).ConfigureAwait(false);
    }

    protected async Task<bool> BookExistsAsync(Guid id)
    {
        using var scope = Fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        return await db.Books.AnyAsync(b => b.Id == id).ConfigureAwait(false);
    }

    protected async Task<bool> MemberExistsAsync(Guid id)
    {
        using var scope = Fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        return await db.Members.AnyAsync(m => m.Id == id).ConfigureAwait(false);
    }

    protected async Task<bool> LoanExistsAsync(Guid id)
    {
        using var scope = Fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        return await db.Loans.AnyAsync(l => l.Id == id).ConfigureAwait(false);
    }

    protected async Task<Loan?> GetLoanEntityAsync(Guid id)
    {
        using var scope = Fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        return await db
            .Loans.AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id)
            .ConfigureAwait(false);
    }

    protected async Task<Guid> PersistBookWithEmbeddingAsync(
        string title,
        string isbn,
        string genre,
        Guid authorId
    )
    {
        using var scope = Fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        var embeddingGenerator = scope.ServiceProvider.GetRequiredService<
            IEmbeddingGenerator<string, Embedding<float>>
        >();

        var book = Book.Create(Guid.NewGuid(), title, isbn, authorId, genre);
        db.Books.Add(book);
        await db.SaveChangesAsync().ConfigureAwait(false);

        var embeddingText = $"{title} {genre}";
        var embeddingResult = await embeddingGenerator.GenerateAsync([embeddingText]);
        var embedding = embeddingResult[0].Vector.ToArray();

        db.Entry(book).Property(BookVectorEmbedding.PropertyName).CurrentValue = new Vector(
            embedding
        );
        await db.SaveChangesAsync().ConfigureAwait(false);

        return book.Id;
    }
}
