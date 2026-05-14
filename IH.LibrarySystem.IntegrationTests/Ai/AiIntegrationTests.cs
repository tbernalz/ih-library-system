using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using IH.LibrarySystem.Application.Ai.Dtos;
using IH.LibrarySystem.IntegrationTests.Abstractions;
using IH.LibrarySystem.IntegrationTests.Collections;
using IH.LibrarySystem.IntegrationTests.Fixtures;

namespace IH.LibrarySystem.IntegrationTests.Ai;

[Collection("Integration")]
public sealed class AiIntegrationTests : BaseIntegrationTest
{
    public AiIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture) { }

    [Fact]
    public async Task Complete_returns_stubbed_model_response()
    {
        var response = await Client.PostAsJsonAsync(
            "/api/ai/complete",
            new CompleteRequest("Hello from integration tests"),
            SerializerOptions
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(SerializerOptions);
        json.GetProperty("response").GetString().Should().StartWith("stub-response:");
    }

    [Fact]
    public async Task SummarizeBook_returns_summary_payload()
    {
        var response = await Client.PostAsJsonAsync(
            "/api/ai/summarize-book",
            new SummarizeBookRequest("Dune", "A desert planet, spice, and politics."),
            SerializerOptions
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(SerializerOptions);
        json.GetProperty("summary").GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task RecommendBooks_returns_recommendations_payload()
    {
        var response = await Client.PostAsJsonAsync(
            "/api/ai/recommend-books",
            new RecommendBooksRequest(
                "I enjoy hard sci-fi with strong world building.",
                new[] { "Science Fiction", "Space Opera" }
            ),
            SerializerOptions
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(SerializerOptions);
        json.GetProperty("recommendations").GetString().Should().NotBeNullOrWhiteSpace();
    }
}
