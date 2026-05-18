using IH.LibrarySystem.Infrastructure.Data;
using Microsoft.Extensions.AI;

namespace IH.LibrarySystem.IntegrationTests.Stubs;

/// <summary>
/// Deterministic embeddings for integration tests (dimensions match the books.vector_embedding column).
/// </summary>
internal sealed class StubEmbeddingGenerator
    : IEmbeddingGenerator<string, Embedding<float>>,
        IDisposable
{
    public EmbeddingGeneratorMetadata Metadata { get; } =
        new(
            "stub",
            new Uri("https://localhost/stub-embeddings"),
            "stub-model",
            BookVectorSchema.Dimensions
        );

    public void Dispose() { }

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values,
        EmbeddingGenerationOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = new GeneratedEmbeddings<Embedding<float>>();
        foreach (var value in values)
        {
            cancellationToken.ThrowIfCancellationRequested();
            result.Add(new Embedding<float>(CreateVector(value)));
        }

        return Task.FromResult(result);
    }

    private static ReadOnlyMemory<float> CreateVector(string value)
    {
        var arr = new float[BookVectorSchema.Dimensions];
        var hash = value.GetHashCode(StringComparison.Ordinal);
        for (var i = 0; i < arr.Length; i++)
        {
            arr[i] = MathF.Sin(hash + i * 0.01f) * 0.01f;
        }

        return arr;
    }
}
