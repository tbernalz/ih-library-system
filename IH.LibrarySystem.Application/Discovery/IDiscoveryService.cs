using IH.LibrarySystem.Application.Books.Dtos;
using IH.LibrarySystem.Application.Discovery.Dtos;
using IH.LibrarySystem.Domain.Books;

namespace IH.LibrarySystem.Application.Discovery;

public interface IDiscoveryService
{
    Task<IReadOnlyList<BookDiscovery>> SearchSemanticAsync(
        string query,
        CancellationToken cancellationToken = default
    );

    Task<DiscoveryChatResponse> ChatAsync(
        string query,
        CancellationToken cancellationToken = default
    );
}
