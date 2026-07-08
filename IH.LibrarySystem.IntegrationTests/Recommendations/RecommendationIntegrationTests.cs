using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IH.LibrarySystem.Application.Recommendations.Dtos;
using IH.LibrarySystem.Domain.Books;
using IH.LibrarySystem.IntegrationTests.Abstractions;
using IH.LibrarySystem.IntegrationTests.Auth;
using IH.LibrarySystem.IntegrationTests.Collections;
using IH.LibrarySystem.IntegrationTests.Fixtures;
using IH.LibrarySystem.IntegrationTests.Support;

namespace IH.LibrarySystem.IntegrationTests.Recommendations;

[Collection("Integration")]
public sealed class RecommendationIntegrationTests : BaseIntegrationTest
{
    public RecommendationIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture) { }

    [Fact]
    public async Task GetRecommendations_WhenMemberHasLoanHistory_ReturnsRecommendations()
    {
        var authorId = await PersistAuthorAsync(
            "Rec Author",
            TestDataFactory.UniqueEmail("rec-author")
        );
        var memberId = await PersistMemberAsync(
            "Rec Member",
            TestDataFactory.UniqueEmail("rec-member")
        );

        var readBookId = await PersistBookWithEmbeddingAsync(
            "Dune",
            TestDataFactory.Isbn(),
            "Science Fiction",
            authorId
        );

        await PersistLoanAsync(
            readBookId,
            memberId,
            DateTime.UtcNow.AddDays(-20),
            DateTime.UtcNow.AddDays(-6)
        );

        await PersistBookWithEmbeddingAsync(
            "Foundation",
            TestDataFactory.Isbn(),
            "Science Fiction",
            authorId
        );
        await PersistBookWithEmbeddingAsync(
            "Neuromancer",
            TestDataFactory.Isbn(),
            "Cyberpunk",
            authorId
        );

        Client.AsMember(memberId);
        var response = await Client.GetAsync(
            new Uri("/api/members/recommendations", UriKind.Relative)
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<RecommendationsResponse>(
            SerializerOptions
        );
        result.Should().NotBeNull();
        result!.ProfileSummary.Should().NotBeNullOrWhiteSpace();
        result.Recommendations.Should().NotBeEmpty();

        result.Recommendations.Should().NotContain(r => r.Book.Id == readBookId);
    }

    [Fact]
    public async Task GetRecommendations_WhenMemberHasNoLoanHistory_ReturnsColdStartMessage()
    {
        var memberId = await PersistMemberAsync(
            "New Member",
            TestDataFactory.UniqueEmail("new-member")
        );

        Client.AsMember(memberId);
        var response = await Client.GetAsync(
            new Uri("/api/members/recommendations", UriKind.Relative)
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<RecommendationsResponse>(
            SerializerOptions
        );
        result.Should().NotBeNull();
        result!.Recommendations.Should().BeEmpty();
        result.ProfileSummary.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GetRecommendations_WhenMemberNotFound_Returns404()
    {
        var nonExistentMemberId = Guid.NewGuid();
        Client.AsMember(nonExistentMemberId);
        var response = await Client.GetAsync(
            new Uri("/api/members/recommendations", UriKind.Relative)
        );

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetRecommendations_WhenTopKIsZero_Returns400()
    {
        var memberId = await PersistMemberAsync(
            "Validation Member",
            TestDataFactory.UniqueEmail("val-member")
        );

        Client.AsMember(memberId);
        var response = await Client.GetAsync(
            new Uri("/api/members/recommendations?topK=0", UriKind.Relative)
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetRecommendations_WhenTopKExceedsMax_Returns400()
    {
        var memberId = await PersistMemberAsync(
            "Validation Member",
            TestDataFactory.UniqueEmail("val-member-max")
        );

        Client.AsMember(memberId);
        var response = await Client.GetAsync(
            new Uri("/api/members/recommendations?topK=99", UriKind.Relative)
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetRecommendations_ExcludesAlreadyBorrowedBooks()
    {
        var authorId = await PersistAuthorAsync(
            "Excl Author",
            TestDataFactory.UniqueEmail("excl-author")
        );
        var memberId = await PersistMemberAsync(
            "Excl Member",
            TestDataFactory.UniqueEmail("excl-member")
        );

        var bookIds = new List<Guid>();
        for (var i = 0; i < 3; i++)
        {
            var bookId = await PersistBookWithEmbeddingAsync(
                $"Read Book {i}",
                TestDataFactory.Isbn(),
                "Fantasy",
                authorId
            );
            bookIds.Add(bookId);
            await PersistLoanAsync(
                bookId,
                memberId,
                DateTime.UtcNow.AddDays(-30),
                DateTime.UtcNow.AddDays(-i - 1)
            );
        }

        Client.AsMember(memberId);
        var response = await Client.GetAsync(
            new Uri("/api/members/recommendations", UriKind.Relative)
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<RecommendationsResponse>(
            SerializerOptions
        );
        result.Should().NotBeNull();

        foreach (var id in bookIds)
            result!.Recommendations.Should().NotContain(r => r.Book.Id == id);
    }
}
