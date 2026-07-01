using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IH.LibrarySystem.Application.Authors.Dtos;
using IH.LibrarySystem.Application.Books.Dtos;
using IH.LibrarySystem.IntegrationTests.Abstractions;
using IH.LibrarySystem.IntegrationTests.Auth;
using IH.LibrarySystem.IntegrationTests.Collections;
using IH.LibrarySystem.IntegrationTests.Fixtures;
using IH.LibrarySystem.IntegrationTests.Support;

namespace IH.LibrarySystem.IntegrationTests.Authors;

[Collection("Integration")]
public sealed class AuthorIntegrationTests : BaseIntegrationTest
{
    public AuthorIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture) { }

    [Fact]
    public async Task CreateAuthor_returns_created_and_persists_row()
    {
        Client.AsStaff();

        var email = TestDataFactory.UniqueEmail("author");
        var request = new CreateAuthorRequest("Jane Author", email, "Bio line");

        var response = await Client.PostAsJsonAsync("/api/authors", request, SerializerOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var dto = await response.Content.ReadFromJsonAsync<AuthorDto>(SerializerOptions);
        dto.Should().NotBeNull();
        dto!.Email.Should().Be(email);

        var entity = await GetAuthorEntityAsync(dto.Id);
        entity.Should().NotBeNull();
        entity!.Email.Should().Be(email);
        entity.Bio.Should().Be("Bio line");
    }

    [Fact]
    public async Task GetAuthor_returns_404_when_missing()
    {
        var response = await Client.GetAsync(
            new Uri($"/api/authors/{Guid.NewGuid()}", UriKind.Relative)
        );

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateAuthor_returns_400_when_email_already_registered()
    {
        Client.AsStaff();

        var email = TestDataFactory.UniqueEmail("dup-author");
        await PersistAuthorAsync("First", email);

        var response = await Client.PostAsJsonAsync(
            "/api/authors",
            new CreateAuthorRequest("Second", email, null),
            SerializerOptions
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateAuthor_updates_database_row()
    {
        Client.AsStaff();

        var id = await PersistAuthorAsync("Old", TestDataFactory.UniqueEmail("old-author"));
        var newEmail = TestDataFactory.UniqueEmail("new-author");

        var response = await Client.PutAsJsonAsync(
            new Uri($"/api/authors/{id}", UriKind.Relative),
            new UpdateAuthorRequest("New", newEmail, "Updated bio"),
            SerializerOptions
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var entity = await GetAuthorEntityAsync(id);
        entity.Should().NotBeNull();
        entity!.Name.Should().Be("New");
        entity.Email.Should().Be(newEmail);
        entity.Bio.Should().Be("Updated bio");
    }

    [Fact]
    public async Task DeleteAuthor_returns_204_and_removes_row()
    {
        Client.AsAdmin();

        var id = await PersistAuthorAsync(
            "Delete Author",
            TestDataFactory.UniqueEmail("delete-author")
        );

        var response = await Client.DeleteAsync(new Uri($"/api/authors/{id}", UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await AuthorExistsAsync(id)).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAuthor_returns_400_when_author_has_books()
    {
        Client.AsAdmin();

        var authorId = await PersistAuthorAsync(
            "With Books",
            TestDataFactory.UniqueEmail("books-author")
        );
        await PersistBookAsync("Attached", TestDataFactory.Isbn(), "SciFi", authorId);

        var response = await Client.DeleteAsync(
            new Uri($"/api/authors/{authorId}", UriKind.Relative)
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        (await AuthorExistsAsync(authorId)).Should().BeTrue();
    }
}
