using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IH.LibrarySystem.Application.Authors.Dtos;
using IH.LibrarySystem.IntegrationTests.Abstractions;
using IH.LibrarySystem.IntegrationTests.Auth;
using IH.LibrarySystem.IntegrationTests.Collections;
using IH.LibrarySystem.IntegrationTests.Fixtures;
using IH.LibrarySystem.IntegrationTests.Support;

namespace IH.LibrarySystem.IntegrationTests.Authors;

[Collection("Integration")]
public sealed class AuthorCqrsIntegrationTests : BaseIntegrationTest
{
    public AuthorCqrsIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture) { }

    [Fact]
    public async Task CreateAuthor_via_CQRS_returns_created_and_persists_row()
    {
        Client.AsStaff();

        var email = TestDataFactory.UniqueEmail("cqrs-author");
        var request = new CreateAuthorRequest("CQRS Author", email, "CQRS Bio");

        var response = await Client.PostAsJsonAsync("/api/authors", request, SerializerOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var dto = await response.Content.ReadFromJsonAsync<AuthorDto>(SerializerOptions);
        dto.Should().NotBeNull();
        dto!.Email.Should().Be(email);

        var entity = await GetAuthorEntityAsync(dto.Id);
        entity.Should().NotBeNull();
        entity!.Email.Should().Be(email);
        entity.Bio.Should().Be("CQRS Bio");
    }

    [Fact]
    public async Task GetAuthor_via_CQRS_returns_author_when_exists()
    {
        Client.AsStaff();

        var email = TestDataFactory.UniqueEmail("cqrs-get-author");
        var authorId = await PersistAuthorAsync("Get Author", email, "Get Bio");

        var response = await Client.GetAsync($"/api/authors/{authorId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await response.Content.ReadFromJsonAsync<AuthorDto>(SerializerOptions);
        dto.Should().NotBeNull();
        dto!.Id.Should().Be(authorId);
        dto.Name.Should().Be("Get Author");
        dto.Email.Should().Be(email);
        dto.Bio.Should().Be("Get Bio");
    }

    [Fact]
    public async Task GetAuthor_via_CQRS_returns_404_when_missing()
    {
        var response = await Client.GetAsync($"/api/authors/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateAuthor_via_CQRS_returns_400_when_email_already_registered()
    {
        Client.AsStaff();

        var email = TestDataFactory.UniqueEmail("cqrs-dup-author");
        await PersistAuthorAsync("First", email);

        var response = await Client.PostAsJsonAsync(
            "/api/authors",
            new CreateAuthorRequest("Second", email, null),
            SerializerOptions
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
