using System.Numerics.Tensors; // Added for native SIMD/Vector acceleration
using IH.LibrarySystem.Domain.Books;
using IH.LibrarySystem.Infrastructure.Data;
using IH.LibrarySystem.Infrastructure.VectorStore;
using Microsoft.EntityFrameworkCore;

namespace IH.LibrarySystem.IntegrationTests.Stubs;

/// <summary>
/// In-memory stub for Qdrant vector store to avoid requiring a real Qdrant instance in integration tests.
/// </summary>
internal sealed class StubQdrantVectorStore(LibraryDbContext context) : IQdrantVectorStore
{
    private readonly LibraryDbContext _context = context;
    private readonly Dictionary<Guid, float[]> _vectors = [];

    public Task InitializeCollectionAsync(CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task UpsertBookVectorAsync(
        Guid bookId,
        float[] embedding,
        CancellationToken cancellationToken = default
    )
    {
        _vectors[bookId] = embedding;
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<BookDiscovery>> SearchSimilarAsync(
        float[] queryEmbedding,
        int topK,
        CancellationToken cancellationToken = default
    )
    {
        var bookIds = _vectors.Keys.ToList();
        var books = await _context
            .Books.Where(b => bookIds.Contains(b.Id))
            .ToListAsync(cancellationToken);

        var results = books
            .Select(book =>
            {
                var embedding = _vectors[book.Id];
                var similarity = CosineSimilarity(queryEmbedding, embedding);
                return new BookDiscovery(book, similarity);
            })
            .OrderByDescending(d => d.SimilarityScore)
            .Take(topK)
            .ToList();

        return results;
    }

    public Task DeleteBookVectorAsync(Guid bookId, CancellationToken cancellationToken = default)
    {
        _vectors.Remove(bookId);
        return Task.CompletedTask;
    }

    private static float CosineSimilarity(ReadOnlySpan<float> a, ReadOnlySpan<float> b)
    {
        if (a.Length != b.Length)
            return 0f;

        return TensorPrimitives.CosineSimilarity(a, b);
    }
}
