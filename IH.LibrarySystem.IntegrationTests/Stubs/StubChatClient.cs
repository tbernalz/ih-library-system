using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;

namespace IH.LibrarySystem.IntegrationTests.Stubs;

/// <summary>
/// Deterministic stand-in for external LLM providers during integration tests.
/// </summary>
internal sealed class StubChatClient : IChatClient, IDisposable
{
    public ChatClientMetadata Metadata { get; } = new("stub");

    public void Dispose() { }

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public Func<IEnumerable<ChatMessage>, string>? ResponseGenerator { get; set; }

    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        var text = ResponseGenerator?.Invoke(messages) ?? BuildResponse(messages);
        return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, text)));
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        var response = await GetResponseAsync(messages, options, cancellationToken)
            .ConfigureAwait(false);
        if (response.Messages is { Count: > 0 })
        {
            yield return new ChatResponseUpdate(ChatRole.Assistant, response.Messages[0].Text);
        }
    }

    private static string BuildResponse(IEnumerable<ChatMessage> messages)
    {
        var lastUser = messages.LastOrDefault(m => m.Role == ChatRole.User);
        var preview = lastUser?.Text is { Length: > 0 } t ? t[..Math.Min(t.Length, 80)] : "(empty)";
        return $"stub-response:{preview}";
    }
}
