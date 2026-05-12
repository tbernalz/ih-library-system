using FluentAssertions;
using IH.LibrarySystem.Application.Ai;
using IH.LibrarySystem.Application.Ai.Dtos;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace IH.LibrarySystem.Application.Tests.Ai;

public class AiServiceTests
{
    private readonly IChatClient _chatClient;
    private readonly ILogger<AiService> _logger;
    private readonly AiService _sut;

    public AiServiceTests()
    {
        _chatClient = Substitute.For<IChatClient>();
        _logger = Substitute.For<ILogger<AiService>>();
        _sut = new AiService(_chatClient, _logger);
    }

    [Fact]
    public async Task CompleteAsync_ShouldReturnChatResponse_WhenClientSucceeds()
    {
        var request = new CompleteRequest("Translate: Hello");
        var expectedText = "Hola";

        SetupChatResponse(expectedText);

        var result = await _sut.CompleteAsync(request);

        result.Should().Be(expectedText);
    }

    [Fact]
    public async Task CompleteAsync_ShouldHandleNullOrEmptyResponse_ByReturningEmptyString()
    {
        SetupChatResponse(null);

        var result = await _sut.CompleteAsync(new CompleteRequest("Test"));

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SummarizeBookDescriptionAsync_ShouldPassFormattedPromptToClient()
    {
        var request = new SummarizeBookRequest("The Hobbit", "A dragon and a hobbit...");
        SetupChatResponse("Summary");

        await _sut.SummarizeBookDescriptionAsync(request);

        await _chatClient
            .Received(1)
            .GetResponseAsync(
                Arg.Is<IEnumerable<ChatMessage>>(m =>
                    m.Any(c => c.Text != null && c.Text.Contains("The Hobbit"))
                    && m.Any(c => c.Role == ChatRole.User)
                ),
                Arg.Any<ChatOptions>()
            );
    }

    private void SetupChatResponse(string? content)
    {
        var message = new ChatMessage(ChatRole.Assistant, content);
        var response = new ChatResponse(new[] { message });

        _chatClient
            .GetResponseAsync(
                Arg.Any<IEnumerable<ChatMessage>>(),
                Arg.Any<ChatOptions>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromResult(response));
    }
}
