using IH.LibrarySystem.Domain.Books;
using IH.LibrarySystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace IH.LibrarySystem.Infrastructure.VectorStore;

public interface IQdrantVectorStore
{
    Task InitializeCollectionAsync(CancellationToken cancellationToken = default);
    Task UpsertBookVectorAsync(
        Guid bookId,
        float[] embedding,
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<BookDiscovery>> SearchSimilarAsync(
        float[] queryEmbedding,
        int topK,
        CancellationToken cancellationToken = default
    );
    Task DeleteBookVectorAsync(Guid bookId, CancellationToken cancellationToken = default);
}

public sealed class QdrantVectorStore : IQdrantVectorStore
{
    private const string CollectionName = "books";
    private readonly QdrantClient _client;
    private readonly LibraryDbContext _context;
    private readonly int _vectorDimensions = 1536;

    public QdrantVectorStore(LibraryDbContext context, IConfiguration configuration)
    {
        var host = configuration["Qdrant:Host"] ?? "localhost";
        var port = configuration.GetValue<int>("Qdrant:Port", 6333);
        _client = new QdrantClient(host, port: port, https: false);
        _context = context;
    }

    public async Task InitializeCollectionAsync(CancellationToken cancellationToken = default)
    {
        var collections = await _client.ListCollectionsAsync(cancellationToken: cancellationToken);
        if (collections.Any(c => c == CollectionName))
        {
            return;
        }

        await _client.CreateCollectionAsync(
            CollectionName,
            new VectorParams { Size = (uint)_vectorDimensions, Distance = Distance.Cosine },
            cancellationToken: cancellationToken
        );
    }

    public async Task UpsertBookVectorAsync(
        Guid bookId,
        float[] embedding,
        CancellationToken cancellationToken = default
    )
    {
        var book = await _context.Books.FindAsync([bookId], cancellationToken);
        if (book is null)
        {
            return;
        }

        var point = new PointStruct
        {
            Id = new PointId { Uuid = bookId.ToString() },
            Vectors = embedding,
            Payload =
            {
                ["book_id"] = bookId.ToString(),
                ["title"] = book.Title,
                ["isbn"] = book.Isbn,
                ["genre"] = book.Genre,
            },
        };

        await _client.UpsertAsync(CollectionName, [point], cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<BookDiscovery>> SearchSimilarAsync(
        float[] queryEmbedding,
        int topK,
        CancellationToken cancellationToken = default
    )
    {
        var results = await _client.SearchAsync(
            CollectionName,
            queryEmbedding,
            limit: (ulong)topK,
            cancellationToken: cancellationToken
        );

        var bookIds = results.Select(r => Guid.Parse(r.Payload["book_id"].StringValue)).ToList();

        var books = await _context
            .Books.Where(b => bookIds.Contains(b.Id))
            .ToListAsync(cancellationToken);

        var bookMap = books.ToDictionary(b => b.Id);

        return
        [
            .. results
                .Select(r =>
                {
                    var bookId = Guid.Parse(r.Payload["book_id"].StringValue);

                    if (!bookMap.TryGetValue(bookId, out var book))
                    {
                        return null;
                    }

                    var similarity = (float)r.Score;
                    return new BookDiscovery(book, similarity);
                })
                .Where(d => d is not null)
                .Select(d => d!),
        ];
    }

    public async Task DeleteBookVectorAsync(
        Guid bookId,
        CancellationToken cancellationToken = default
    )
    {
        await _client.DeleteAsync(
            CollectionName,
            new PointId { Uuid = bookId.ToString() },
            cancellationToken: cancellationToken
        );
    }
}
