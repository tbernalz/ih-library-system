using FluentAssertions;
using IH.LibrarySystem.Application.Recommendations;
using IH.LibrarySystem.Domain.Books;
using IH.LibrarySystem.Domain.Common;
using IH.LibrarySystem.Domain.Loans;
using IH.LibrarySystem.Domain.Members;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace IH.LibrarySystem.Application.Tests.Recommendations;

public class RecommendationServiceTests
{
    private readonly IMemberRepository _memberRepository;
    private readonly ILoanRepository _loanRepository;
    private readonly IBookDiscoveryRepository _bookDiscoveryRepository;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
    private readonly IChatClient _chatClient;
    private readonly ILogger<RecommendationService> _logger;
    private readonly RecommendationService _sut;

    public RecommendationServiceTests()
    {
        _memberRepository = Substitute.For<IMemberRepository>();
        _loanRepository = Substitute.For<ILoanRepository>();
        _bookDiscoveryRepository = Substitute.For<IBookDiscoveryRepository>();
        _embeddingGenerator = Substitute.For<IEmbeddingGenerator<string, Embedding<float>>>();
        _chatClient = Substitute.For<IChatClient>();
        _logger = Substitute.For<ILogger<RecommendationService>>();

        _sut = new RecommendationService(
            _memberRepository,
            _loanRepository,
            _bookDiscoveryRepository,
            _embeddingGenerator,
            _chatClient,
            _logger
        );
    }

    #region GetRecommendationsAsync — Guard Tests

    [Fact]
    public async Task GetRecommendationsAsync_WhenMemberNotFound_ThrowsKeyNotFoundException()
    {
        var memberId = Guid.NewGuid();
        _memberRepository.GetByIdAsync(memberId).Returns((Member?)null);

        var act = () => _sut.GetRecommendationsAsync(memberId);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GetRecommendationsAsync_WhenTopKIsZero_ThrowsArgumentOutOfRangeException()
    {
        var memberId = Guid.NewGuid();
        _memberRepository
            .GetByIdAsync(memberId)
            .Returns(Member.Create(memberId, "Test", "t@t.com"));

        var act = () => _sut.GetRecommendationsAsync(memberId, topK: 0);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    #endregion

    #region GetRecommendationsAsync — Cold Start

    [Fact]
    public async Task GetRecommendationsAsync_WhenNoLoanHistory_ReturnsColdStartResponse()
    {
        var memberId = Guid.NewGuid();
        _memberRepository
            .GetByIdAsync(memberId)
            .Returns(Member.Create(memberId, "Test", "t@t.com"));
        _loanRepository
            .SearchAsync(Arg.Any<LoanSearchFilter>())
            .Returns(new PagedResult<Loan>([], 0, 1, 200));

        var result = await _sut.GetRecommendationsAsync(memberId);

        result.Recommendations.Should().BeEmpty();
        result.ProfileSummary.Should().NotBeNullOrWhiteSpace();

        await _embeddingGenerator
            .DidNotReceive()
            .GenerateAsync(
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<EmbeddingGenerationOptions>(),
                Arg.Any<CancellationToken>()
            );
    }

    #endregion

    #region GetRecommendationsAsync — Happy Path

    [Fact]
    public async Task GetRecommendationsAsync_WhenHistoryExists_ReturnsRecommendationsExcludingReadBooks()
    {
        var memberId = Guid.NewGuid();
        var member = Member.Create(memberId, "Alice", "alice@test.com");
        _memberRepository.GetByIdAsync(memberId).Returns(member);

        var readBookId = Guid.NewGuid();
        var readBook = Book.Create(readBookId, "Read Book", "111", Guid.NewGuid(), "Fantasy");
        var loan = CreateLoanWithBook(readBookId, memberId, readBook);

        _loanRepository
            .SearchAsync(Arg.Any<LoanSearchFilter>())
            .Returns(new PagedResult<Loan>([loan], 1, 1, 200));

        var candidateId = Guid.NewGuid();
        var candidateBook = Book.Create(candidateId, "New Book", "222", Guid.NewGuid(), "Fantasy");
        var discoveries = new List<BookDiscovery>
        {
            new(readBook, 0.99f),
            new(candidateBook, 0.85f),
        };

        SetupEmbeddingGenerator();
        _bookDiscoveryRepository
            .SearchByVectorSimilarityAsync(
                Arg.Any<float[]>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(discoveries);

        SetupChatResponse("PROFILE: Loves fantasy.\nREASON1: Great match for fantasy fans.");

        var result = await _sut.GetRecommendationsAsync(memberId, topK: 5);

        result.Recommendations.Should().ContainSingle();
        result.Recommendations[0].Book.Id.Should().Be(candidateId);
        result.Recommendations[0].Reason.Should().NotBeNullOrWhiteSpace();
        result.ProfileSummary.Should().Contain("fantasy");
    }

    [Fact]
    public async Task GetRecommendationsAsync_WhenAllCandidatesAlreadyRead_ReturnsEmptyList()
    {
        var memberId = Guid.NewGuid();
        var member = Member.Create(memberId, "Bob", "bob@test.com");
        _memberRepository.GetByIdAsync(memberId).Returns(member);

        var readBookId = Guid.NewGuid();
        var readBook = Book.Create(readBookId, "Read Book", "111", Guid.NewGuid(), "SciFi");
        var loan = CreateLoanWithBook(readBookId, memberId, readBook);

        _loanRepository
            .SearchAsync(Arg.Any<LoanSearchFilter>())
            .Returns(new PagedResult<Loan>([loan], 1, 1, 200));

        _bookDiscoveryRepository
            .SearchByVectorSimilarityAsync(
                Arg.Any<float[]>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(new List<BookDiscovery> { new(readBook, 0.99f) });

        SetupEmbeddingGenerator();

        var result = await _sut.GetRecommendationsAsync(memberId, topK: 5);

        result.Recommendations.Should().BeEmpty();
        result.ProfileSummary.Should().NotBeNullOrWhiteSpace();

        await _chatClient
            .DidNotReceive()
            .GetResponseAsync(
                Arg.Any<IEnumerable<ChatMessage>>(),
                Arg.Any<ChatOptions>(),
                Arg.Any<CancellationToken>()
            );
    }

    #endregion

    #region Helpers

    private void SetupEmbeddingGenerator()
    {
        var vector = new float[1536];
        var embedding = new Embedding<float>(vector);
        var generated = new GeneratedEmbeddings<Embedding<float>> { embedding };

        _embeddingGenerator
            .GenerateAsync(
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<EmbeddingGenerationOptions>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromResult(generated));
    }

    private void SetupChatResponse(string text)
    {
        var message = new ChatMessage(ChatRole.Assistant, text);
        var response = new ChatResponse(new[] { message });

        _chatClient
            .GetResponseAsync(
                Arg.Any<IEnumerable<ChatMessage>>(),
                Arg.Any<ChatOptions>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromResult(response));
    }

    private static Loan CreateLoanWithBook(Guid bookId, Guid memberId, Book book)
    {
        var loan = Loan.Create(
            Guid.NewGuid(),
            bookId,
            memberId,
            DateTime.UtcNow.AddDays(-20),
            DateTime.UtcNow.AddDays(-6)
        );

        typeof(Loan).GetProperty("Book")?.SetValue(loan, book);
        return loan;
    }

    #endregion
}
