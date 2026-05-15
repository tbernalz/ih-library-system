namespace IH.LibrarySystem.Infrastructure.Data;

/// <summary>
/// Fixed embedding width for the <c>books.vector_embedding</c> column. Must match <see cref="IH.LibrarySystem.Application.Configuration.DiscoverySettings.EmbeddingDimensions"/> and migrations.
/// </summary>
public static class BookVectorSchema
{
    public const int Dimensions = 1536;
}
