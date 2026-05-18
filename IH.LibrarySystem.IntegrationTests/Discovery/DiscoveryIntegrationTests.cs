using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IH.LibrarySystem.Application.Discovery.Dtos;
using IH.LibrarySystem.IntegrationTests.Abstractions;
using IH.LibrarySystem.IntegrationTests.Collections;
using IH.LibrarySystem.IntegrationTests.Fixtures;
using IH.LibrarySystem.IntegrationTests.Support;

namespace IH.LibrarySystem.IntegrationTests.Discovery;

[Collection("Integration")]
public sealed class DiscoveryIntegrationTests : BaseIntegrationTest
{
    public DiscoveryIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture) { }

    [Fact]
    public async Task Chat_returns_response_when_books_have_embeddings()
    {
        var authorId = await PersistAuthorAsync(
            "Sci-Fi Author",
            TestDataFactory.UniqueEmail("sci-fi-author")
        );

        await PersistBookWithEmbeddingAsync(
            "Dune",
            TestDataFactory.Isbn(),
            "Science Fiction",
            authorId
        );

        await PersistBookWithEmbeddingAsync(
            "Foundation",
            TestDataFactory.Isbn(),
            "Science Fiction",
            authorId
        );

        var response = await Client.PostAsJsonAsync(
            "/api/discovery/chat",
            new DiscoveryChatRequest("I enjoy science fiction about space"),
            SerializerOptions
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<DiscoveryChatResponse>(
            SerializerOptions
        );
        result.Should().NotBeNull();
        result!.Explanation.Should().NotBeNullOrWhiteSpace();
        result.Books.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Chat_returns_message_when_no_books_have_embeddings()
    {
        var authorId = await PersistAuthorAsync(
            "Test Author",
            TestDataFactory.UniqueEmail("test-author")
        );

        await PersistBookAsync("Test Book", TestDataFactory.Isbn(), "Genre", authorId);

        var response = await Client.PostAsJsonAsync(
            "/api/discovery/chat",
            new DiscoveryChatRequest("I enjoy science fiction about space"),
            SerializerOptions
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<DiscoveryChatResponse>(
            SerializerOptions
        );
        result.Should().NotBeNull();
        result!.Explanation.Should().Contain("No indexed books were found");
        result.Books.Should().BeEmpty();
    }

    [Fact]
    public async Task Chat_returns_400_when_query_is_empty()
    {
        var response = await Client.PostAsJsonAsync(
            "/api/discovery/chat",
            new DiscoveryChatRequest(""),
            SerializerOptions
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Chat_returns_400_when_query_is_whitespace()
    {
        var response = await Client.PostAsJsonAsync(
            "/api/discovery/chat",
            new DiscoveryChatRequest("   "),
            SerializerOptions
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
