using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using IH.LibrarySystem.Application.Ai.Dtos;
using IH.LibrarySystem.IntegrationTests.Abstractions;
using IH.LibrarySystem.IntegrationTests.Auth;
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
        Client.AsStaff();

        var response = await Client.PostAsJsonAsync(
            "/api/ai/complete",
            new CompleteRequest("Hello from integration tests"),
            SerializerOptions
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(SerializerOptions);
        json.GetProperty("response").GetString().Should().StartWith("stub-response:");
    }
}
