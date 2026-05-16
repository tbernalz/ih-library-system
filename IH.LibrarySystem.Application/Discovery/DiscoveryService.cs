using System.Text;
using IH.LibrarySystem.Application.Books.Dtos;
using IH.LibrarySystem.Application.Discovery.Dtos;
using IH.LibrarySystem.Domain.Books;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace IH.LibrarySystem.Application.Discovery;

public sealed class DiscoveryService(
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    IBookDiscoveryRepository bookDiscoveryRepository,
    IChatClient chatClient,
    ILogger<DiscoveryService> logger
) : IDiscoveryService
{
    public async Task<IReadOnlyList<BookDiscovery>> SearchSemanticAsync(
        string query,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);

        var embeddingResult = await embeddingGenerator.GenerateAsync(
            [query],
            cancellationToken: cancellationToken
        );

        if (embeddingResult.Count == 0)
        {
            throw new InvalidOperationException("Embedding provider returned no vectors.");
        }

        var vector = embeddingResult[0].Vector;
        if (vector.IsEmpty)
        {
            throw new InvalidOperationException("Embedding provider returned an empty vector.");
        }

        var queryEmbedding = vector.ToArray();
        return await bookDiscoveryRepository.SearchByVectorSimilarityAsync(
            queryEmbedding,
            topK: 5,
            cancellationToken
        );
    }

    public async Task<DiscoveryChatResponse> ChatAsync(
        string query,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);

        var matches = await SearchSemanticAsync(query, cancellationToken);
        if (matches.Count == 0)
        {
            return new DiscoveryChatResponse(
                "No indexed books were found for semantic search. Ensure embeddings exist (run ingestion or wait for the background indexer).",
                []
            );
        }

        var books = matches.Select(m => MapToDto(m.Book)).ToList();
        var prompt = BuildAugmentationPrompt(query, matches);

        logger.LogDebug("Discovery chat augmentation prompt length: {Length}", prompt.Length);

        var response = await chatClient.GetResponseAsync(
            prompt,
            cancellationToken: cancellationToken
        );
        var explanation = response.Messages.FirstOrDefault()?.Text ?? string.Empty;

        return new DiscoveryChatResponse(explanation.Trim(), books);
    }

    private static string BuildAugmentationPrompt(
        string userQuery,
        IReadOnlyList<BookDiscovery> matches
    )
    {
        var sb = new StringBuilder();
        sb.AppendLine("You are a library discovery copilot. The user is looking for books.");
        sb.AppendLine("Here is their request:");
        sb.AppendLine(userQuery.Trim());
        sb.AppendLine();
        sb.AppendLine(
            "The following books were retrieved by semantic similarity (metadata only). "
                + "Explain briefly why each title plausibly fits the user's intent, without inventing plot details not implied by the metadata."
        );
        sb.AppendLine();

        for (var i = 0; i < matches.Count; i++)
        {
            var m = matches[i];
            sb.AppendLine(
                $"{i + 1}. Title: {m.Book.Title}; Genre: {m.Book.Genre}; ISBN: {m.Book.Isbn}; Status: {m.Book.Status}; Similarity score: {m.SimilarityScore:F4}"
            );
        }

        sb.AppendLine();
        sb.AppendLine("Write a concise, friendly answer for the patron (a few short paragraphs).");
        return sb.ToString();
    }

    private static BookDto MapToDto(Book book) =>
        new()
        {
            Id = book.Id,
            Title = book.Title,
            Isbn = book.Isbn,
            Status = book.Status,
            AuthorId = book.AuthorId,
        };
}
