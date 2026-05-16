namespace IH.LibrarySystem.Application.Discovery;

using System.Threading.Channels;

public sealed class DiscoveryIngestionQueue
{
    private readonly Channel<bool> _signal = Channel.CreateUnbounded<bool>();

    public ValueTask NotifyNewBookAsync() => _signal.Writer.WriteAsync(true);

    public IAsyncEnumerable<bool> Reader => _signal.Reader.ReadAllAsync();
}
