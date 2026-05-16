using IH.LibrarySystem.Domain.Books;
using IH.LibrarySystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.VectorData;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace IH.LibrarySystem.Infrastructure.Repositories;

public sealed class BookDiscoveryRepository(LibraryDbContext context) : IBookDiscoveryRepository
{
    public async Task<IReadOnlyList<BookDiscovery>> SearchByVectorSimilarityAsync(
        float[] queryEmbedding,
        int topK,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(queryEmbedding);
        ArgumentOutOfRangeException.ThrowIfLessThan(topK, 1);

        var queryVector = new Vector(queryEmbedding);

        var rows = await context
            .Books.AsNoTracking()
            .Where(b => EF.Property<Vector>(b, BookVectorEmbedding.PropertyName) != null)
            .Select(b => new
            {
                Book = b,
                Distance = EF.Property<Vector>(b, BookVectorEmbedding.PropertyName)
                    .CosineDistance(queryVector),
            })
            .OrderBy(x => x.Distance)
            .Take(topK)
            .ToListAsync(cancellationToken);

        var vectorResults = rows.Select(r => new VectorSearchResult<Book>(
                r.Book,
                SimilarityFromCosineDistance(r.Distance)
            ))
            .ToList();

        return vectorResults
            .Select(v => new BookDiscovery(v.Record, (float)(v.Score ?? 0d)))
            .ToList();
    }

    public async Task<IReadOnlyList<Book>> GetBooksMissingEmbeddingsAsync(
        CancellationToken cancellationToken = default
    ) =>
        await context
            .Books.AsTracking()
            .Where(b => EF.Property<Vector>(b, BookVectorEmbedding.PropertyName) == null)
            .ToListAsync(cancellationToken);

    public async Task UpdateEmbeddingAsync(
        Guid bookId,
        float[] embedding,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(embedding);

        var book = await context.Books.FirstOrDefaultAsync(b => b.Id == bookId, cancellationToken);
        if (book is null)
        {
            return;
        }

        context.Entry(book).Property(BookVectorEmbedding.PropertyName).CurrentValue = new Vector(
            embedding
        );
    }

    /// <summary>
    /// Converts pgvector cosine distance to a higher-is-better score in roughly [0,1] for normalized embeddings.
    /// </summary>
    private static float SimilarityFromCosineDistance(double distance)
    {
        var similarity = 1f - (float)distance;
        return similarity < 0f ? 0f : (similarity > 1f ? 1f : similarity);
    }
}
