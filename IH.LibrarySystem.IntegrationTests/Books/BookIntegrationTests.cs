using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IH.LibrarySystem.Application.Books.Dtos;
using IH.LibrarySystem.Domain.Books;
using IH.LibrarySystem.Domain.Common;
using IH.LibrarySystem.IntegrationTests.Abstractions;
using IH.LibrarySystem.IntegrationTests.Collections;
using IH.LibrarySystem.IntegrationTests.Fixtures;
using IH.LibrarySystem.IntegrationTests.Support;

namespace IH.LibrarySystem.IntegrationTests.Books;

[Collection("Integration")]
public sealed class BookIntegrationTests : BaseIntegrationTest
{
    public BookIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture) { }

    [Fact]
    public async Task CreateBook_returns_created_and_persists_row()
    {
        var authorId = await PersistAuthorAsync(
            "Book Author",
            TestDataFactory.UniqueEmail("book-author")
        );
        var isbn = TestDataFactory.Isbn();
        var request = new CreateBookRequest("Integration Title", isbn, "Fantasy", authorId);

        var response = await Client.PostAsJsonAsync("/api/books", request, SerializerOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var dto = await response.Content.ReadFromJsonAsync<BookDto>(SerializerOptions);
        dto.Should().NotBeNull();
        dto!.Isbn.Should().Be(isbn);
        dto.Status.Should().Be(BookStatus.Available);

        var entity = await GetBookEntityAsync(dto.Id);
        entity.Should().NotBeNull();
        entity!.AuthorId.Should().Be(authorId);
    }

    [Fact]
    public async Task GetBook_returns_404_when_missing()
    {
        var response = await Client.GetAsync(
            new Uri($"/api/books/{Guid.NewGuid()}", UriKind.Relative)
        );

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateBook_returns_404_when_author_does_not_exist()
    {
        var request = new CreateBookRequest(
            "Orphan",
            TestDataFactory.Isbn(),
            "Genre",
            Guid.NewGuid()
        );

        var response = await Client.PostAsJsonAsync("/api/books", request, SerializerOptions);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateBook_returns_400_when_isbn_already_exists()
    {
        var authorId = await PersistAuthorAsync(
            "ISBN Author",
            TestDataFactory.UniqueEmail("isbn-author")
        );
        var isbn = TestDataFactory.Isbn();
        await PersistBookAsync("First Copy", isbn, "Genre", authorId);

        var response = await Client.PostAsJsonAsync(
            "/api/books",
            new CreateBookRequest("Second Copy", isbn, "Genre", authorId),
            SerializerOptions
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SearchBooks_returns_paged_payload()
    {
        var authorId = await PersistAuthorAsync(
            "Search Author",
            TestDataFactory.UniqueEmail("search-author")
        );
        await PersistBookAsync("UniqueAlphaTitle", TestDataFactory.Isbn(), "Genre", authorId);

        var response = await Client.GetAsync(
            new Uri(
                "/api/books/search?searchTerm=UniqueAlphaTitle&pageNumber=1&pageSize=5",
                UriKind.Relative
            )
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var page = await response.Content.ReadFromJsonAsync<PagedResult<BookDto>>(
            SerializerOptions
        );
        page.Should().NotBeNull();
        page!.Items.Should().ContainSingle(b => b.Title == "UniqueAlphaTitle");
        page.TotalCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task UpdateBook_updates_database_row()
    {
        var authorId = await PersistAuthorAsync(
            "Update Author",
            TestDataFactory.UniqueEmail("update-author")
        );
        var id = await PersistBookAsync("Old Title", TestDataFactory.Isbn(), "OldGenre", authorId);

        var response = await Client.PutAsJsonAsync(
            new Uri($"/api/books/{id}", UriKind.Relative),
            new UpdateBookRequest("New Title", TestDataFactory.Isbn(), "NewGenre"),
            SerializerOptions
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var entity = await GetBookEntityAsync(id);
        entity.Should().NotBeNull();
        entity!.Title.Should().Be("New Title");
        entity.Genre.Should().Be("NewGenre");
    }

    [Fact]
    public async Task DeleteBook_removes_row_from_database()
    {
        var authorId = await PersistAuthorAsync(
            "Delete Author",
            TestDataFactory.UniqueEmail("delete-book-author")
        );
        var id = await PersistBookAsync("Delete Me", TestDataFactory.Isbn(), "Genre", authorId);

        var response = await Client.DeleteAsync(new Uri($"/api/books/{id}", UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await BookExistsAsync(id)).Should().BeFalse();
    }
}
