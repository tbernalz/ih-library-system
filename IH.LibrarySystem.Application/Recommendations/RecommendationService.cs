using System.Text;
using IH.LibrarySystem.Application.Books.Dtos;
using IH.LibrarySystem.Application.Discovery.Helpers;
using IH.LibrarySystem.Application.Recommendations.Dtos;
using IH.LibrarySystem.Domain.Books;
using IH.LibrarySystem.Domain.Loans;
using IH.LibrarySystem.Domain.Members;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace IH.LibrarySystem.Application.Recommendations;

public sealed class RecommendationService(
    IMemberRepository memberRepository,
    ILoanRepository loanRepository,
    IBookDiscoveryRepository bookDiscoveryRepository,
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    IChatClient chatClient,
    ILogger<RecommendationService> logger
) : IRecommendationService
{
    private const int CandidateMultiplier = 4;

    public async Task<RecommendationsResponse> GetRecommendationsAsync(
        Guid memberId,
        int topK = 5,
        CancellationToken cancellationToken = default
    )
    {
        if (topK <= 0)
            throw new ArgumentOutOfRangeException(nameof(topK), "topK must be greater than zero.");

        var member = await memberRepository.GetByIdAsync(memberId);
        if (member is null)
            throw new KeyNotFoundException($"Member with ID {memberId} not found.");

        var historyFilter = new LoanSearchFilter(MemberId: memberId, PageNumber: 1, PageSize: 200);
        var loanPage = await loanRepository.SearchAsync(historyFilter);
        var borrowedBookIds = loanPage.Items.Select(l => l.BookId).ToHashSet();

        logger.LogDebug(
            "Member {MemberId} has {Count} books in loan history.",
            memberId,
            borrowedBookIds.Count
        );

        if (borrowedBookIds.Count == 0)
        {
            logger.LogInformation(
                "Member {MemberId} has no loan history — returning cold-start response.",
                memberId
            );
            return new RecommendationsResponse(
                "You haven't borrowed any books yet. Start exploring our collection to get personalised recommendations!",
                []
            );
        }

        var profileText = BuildProfileText(loanPage.Items.Select(l => l.Book).ToList());

        var embeddingResult = await embeddingGenerator.GenerateAsync(
            [profileText],
            cancellationToken: cancellationToken
        );

        if (embeddingResult.Count == 0 || embeddingResult[0].Vector.IsEmpty)
            throw new InvalidOperationException("Embedding provider returned an empty vector.");

        var profileVector = embeddingResult[0].Vector.ToArray();

        var candidates = await bookDiscoveryRepository.SearchByVectorSimilarityAsync(
            profileVector,
            topK: topK * CandidateMultiplier,
            cancellationToken
        );

        var filtered = candidates
            .Where(c => !borrowedBookIds.Contains(c.Book.Id))
            .Take(topK)
            .ToList();

        logger.LogDebug(
            "Candidates before/after filtering: {Before}/{After}",
            candidates.Count,
            filtered.Count
        );

        if (filtered.Count == 0)
        {
            return new RecommendationsResponse(
                "It looks like you've already read everything we have that matches your taste — impressive! Check back after new books arrive.",
                []
            );
        }

        var prompt = BuildPrompt(
            member.Name,
            loanPage.Items.Select(l => l.Book).ToList(),
            filtered.Select(c => c.Book).ToList()
        );

        logger.LogDebug(
            "Sending recommendation prompt for member {MemberId} (prompt length {Length}).",
            memberId,
            prompt.Length
        );

        var chatResponse = await chatClient.GetResponseAsync(
            prompt,
            cancellationToken: cancellationToken
        );

        var llmText = chatResponse.Messages.FirstOrDefault()?.Text ?? string.Empty;

        var reasons = ParseReasons(llmText, filtered.Count);
        var profileSummary = ExtractProfileSummary(llmText);

        var recommendations = filtered
            .Select(
                (c, i) =>
                    new RecommendationDto
                    {
                        Book = MapToDto(c.Book),
                        SimilarityScore = c.SimilarityScore,
                        Reason =
                            i < reasons.Count
                                ? reasons[i]
                                : "A great match for your reading taste.",
                    }
            )
            .ToList();

        return new RecommendationsResponse(profileSummary, recommendations);
    }

    private static string BuildProfileText(List<Book> books)
    {
        var sb = new StringBuilder("Books I have read: ");
        sb.AppendJoin(", ", books.Select(BookEmbeddingText.Format));
        return sb.ToString();
    }

    private static string BuildPrompt(string memberName, List<Book> history, List<Book> candidates)
    {
        var sb = new StringBuilder();
        sb.AppendLine(
            "You are a librarian recommending books to a member. Be concise and friendly."
        );
        sb.AppendLine();
        sb.AppendLine($"Member: {memberName}");
        sb.AppendLine("Books they have already read:");
        for (var i = 0; i < history.Count; i++)
            sb.AppendLine($"  {i + 1}. {history[i].Title} ({history[i].Genre})");

        sb.AppendLine();
        sb.AppendLine(
            "Candidate books retrieved by semantic similarity (not yet read by the member):"
        );
        for (var i = 0; i < candidates.Count; i++)
            sb.AppendLine(
                $"  {i + 1}. {candidates[i].Title} | Genre: {candidates[i].Genre} | ISBN: {candidates[i].Isbn}"
            );

        sb.AppendLine();
        sb.AppendLine(
            "Respond in exactly this format (no extra text, no markdown, no numbering outside the markers):"
        );
        sb.AppendLine("PROFILE: <one sentence describing the member's reading taste>");
        for (var i = 0; i < candidates.Count; i++)
            sb.AppendLine($"REASON{i + 1}: <one sentence why book {i + 1} suits this member>");

        return sb.ToString();
    }

    private static string ExtractProfileSummary(string llmText)
    {
        foreach (var line in llmText.Split('\n'))
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("PROFILE:", StringComparison.OrdinalIgnoreCase))
                return trimmed["PROFILE:".Length..].Trim();
        }

        return "A reader with eclectic tastes.";
    }

    private static List<string> ParseReasons(string llmText, int count)
    {
        var reasons = new List<string>(count);
        for (var i = 1; i <= count; i++)
        {
            var marker = $"REASON{i}:";
            foreach (var line in llmText.Split('\n'))
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith(marker, StringComparison.OrdinalIgnoreCase))
                {
                    reasons.Add(trimmed[marker.Length..].Trim());
                    break;
                }
            }
        }

        return reasons;
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
