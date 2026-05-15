namespace IH.LibrarySystem.Domain.Books;

/// <summary>
/// A book returned from semantic discovery with a vector similarity score (higher is more similar).
/// </summary>
public sealed record BookDiscovery(Book Book, float SimilarityScore);
