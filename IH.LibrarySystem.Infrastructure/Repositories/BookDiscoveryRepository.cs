using IH.LibrarySystem.Domain.Books;
using IH.LibrarySystem.Infrastructure.Data;
using IH.LibrarySystem.Infrastructure.VectorStore;
using Microsoft.EntityFrameworkCore;

namespace IH.LibrarySystem.Infrastructure.Repositories;

public sealed class BookDiscoveryRepository(
    LibraryDbContext context,
    IQdrantVectorStore vectorStore
) : IBookDiscoveryRepository
{
    public async Task<IReadOnlyList<BookDiscovery>> SearchByVectorSimilarityAsync(
        float[] queryEmbedding,
        int topK,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(queryEmbedding);
        ArgumentOutOfRangeException.ThrowIfLessThan(topK, 1);

        return await vectorStore.SearchSimilarAsync(queryEmbedding, topK, cancellationToken);
    }

    public async Task<IReadOnlyList<Book>> GetBooksMissingEmbeddingsAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await context
            .Books.AsTracking()
            .Where(b => !b.HasEmbedding)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateEmbeddingAsync(
        Guid bookId,
        float[] embedding,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(embedding);

        await vectorStore.UpsertBookVectorAsync(bookId, embedding, cancellationToken);

        var book = await context.Books.FindAsync([bookId], cancellationToken);
        book?.MarkAsHasEmbedding();
    }
}
