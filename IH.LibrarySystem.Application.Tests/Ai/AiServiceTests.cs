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
