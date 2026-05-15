namespace IH.LibrarySystem.Domain.Books;

public interface IBookDiscoveryRepository
{
    Task<IReadOnlyList<BookDiscovery>> SearchByVectorSimilarityAsync(
        float[] queryEmbedding,
        int topK,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<Book>> GetBooksMissingEmbeddingsAsync(
        CancellationToken cancellationToken = default
    );

    Task UpdateEmbeddingAsync(
        Guid bookId,
        float[] embedding,
        CancellationToken cancellationToken = default
    );
}
