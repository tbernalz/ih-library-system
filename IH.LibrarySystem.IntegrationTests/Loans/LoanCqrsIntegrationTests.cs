using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IH.LibrarySystem.Application.Loans.Dtos;
using IH.LibrarySystem.Domain.Books;
using IH.LibrarySystem.Domain.Common;
using IH.LibrarySystem.IntegrationTests.Abstractions;
using IH.LibrarySystem.IntegrationTests.Auth;
using IH.LibrarySystem.IntegrationTests.Collections;
using IH.LibrarySystem.IntegrationTests.Fixtures;
using IH.LibrarySystem.IntegrationTests.Support;

namespace IH.LibrarySystem.IntegrationTests.Loans;

[Collection("Integration")]
public sealed class LoanCqrsIntegrationTests : BaseIntegrationTest
{
    public LoanCqrsIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture) { }

    [Fact]
    public async Task CheckoutBook_via_CQRS_creates_loan_and_marks_book_as_loaned()
    {
        Client.AsStaff();

        var authorId = await PersistAuthorAsync(
            "CQRS Loan Author",
            TestDataFactory.UniqueEmail("cqrs-loan-author")
        );
        var bookId = await PersistBookAsync(
            "CQRS Loanable",
            TestDataFactory.Isbn(),
            "Genre",
            authorId
        );
        var memberId = await PersistMemberAsync(
            "CQRS Borrower",
            TestDataFactory.UniqueEmail("cqrs-borrower")
        );

        var response = await Client.PostAsJsonAsync(
            "/api/loans/checkout",
            new CheckoutBookRequest(bookId, memberId),
            SerializerOptions
        );

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var dto = await response.Content.ReadFromJsonAsync<LoanDto>(SerializerOptions);
        dto.Should().NotBeNull();
        dto!.BookId.Should().Be(bookId);
        dto.MemberId.Should().Be(memberId);
        dto.ReturnDate.Should().BeNull();

        var book = await GetBookEntityAsync(bookId);
        book.Should().NotBeNull();
        book!.Status.Should().Be(BookStatus.Loaned);
    }

    [Fact]
    public async Task CheckoutBook_via_CQRS_returns_400_when_book_is_not_available()
    {
        Client.AsStaff();

        var authorId = await PersistAuthorAsync(
            "CQRS Busy Author",
            TestDataFactory.UniqueEmail("cqrs-busy-author")
        );
        var bookId = await PersistBookAsync(
            "CQRS Busy Book",
            TestDataFactory.Isbn(),
            "Genre",
            authorId
        );
        var memberA = await PersistMemberAsync(
            "CQRS Member A",
            TestDataFactory.UniqueEmail("cqrs-member-a")
        );
        var memberB = await PersistMemberAsync(
            "CQRS Member B",
            TestDataFactory.UniqueEmail("cqrs-member-b")
        );

        var first = await Client.PostAsJsonAsync(
            "/api/loans/checkout",
            new CheckoutBookRequest(bookId, memberA),
            SerializerOptions
        );
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        var second = await Client.PostAsJsonAsync(
            "/api/loans/checkout",
            new CheckoutBookRequest(bookId, memberB),
            SerializerOptions
        );

        second.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CheckoutBook_via_CQRS_returns_400_when_member_not_found()
    {
        Client.AsStaff();

        var authorId = await PersistAuthorAsync(
            "CQRS Missing Author",
            TestDataFactory.UniqueEmail("cqrs-missing-author")
        );
        var bookId = await PersistBookAsync(
            "CQRS Missing Book",
            TestDataFactory.Isbn(),
            "Genre",
            authorId
        );
        var nonExistentMemberId = Guid.NewGuid();

        var response = await Client.PostAsJsonAsync(
            "/api/loans/checkout",
            new CheckoutBookRequest(bookId, nonExistentMemberId),
            SerializerOptions
        );

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CheckoutBook_via_CQRS_returns_400_when_book_not_found()
    {
        Client.AsStaff();

        var memberId = await PersistMemberAsync(
            "CQRS Book Missing",
            TestDataFactory.UniqueEmail("cqrs-book-missing")
        );
        var nonExistentBookId = Guid.NewGuid();

        var response = await Client.PostAsJsonAsync(
            "/api/loans/checkout",
            new CheckoutBookRequest(nonExistentBookId, memberId),
            SerializerOptions
        );

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetLoans_via_CQRS_can_filter_by_member_id()
    {
        Client.AsStaff();

        var authorId = await PersistAuthorAsync(
            "CQRS Filter Author",
            TestDataFactory.UniqueEmail("cqrs-filter-author")
        );
        var bookId = await PersistBookAsync(
            "CQRS Filter Book",
            TestDataFactory.Isbn(),
            "Genre",
            authorId
        );
        var memberId = await PersistMemberAsync(
            "CQRS Filter Member",
            TestDataFactory.UniqueEmail("cqrs-filter-member")
        );

        var checkout = await Client.PostAsJsonAsync(
            "/api/loans/checkout",
            new CheckoutBookRequest(bookId, memberId),
            SerializerOptions
        );
        checkout.StatusCode.Should().Be(HttpStatusCode.Created);

        var response = await Client.GetAsync(
            $"/api/loans?memberId={memberId}&pageNumber=1&pageSize=10"
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var page = await response.Content.ReadFromJsonAsync<PagedResult<LoanDto>>(
            SerializerOptions
        );
        page.Should().NotBeNull();
        page!.Items.Should().NotBeEmpty();
        page.Items.Should().OnlyContain(l => l.MemberId == memberId);
    }

    [Fact]
    public async Task GetLoans_via_CQRS_can_filter_by_book_id()
    {
        Client.AsStaff();

        var authorId = await PersistAuthorAsync(
            "CQRS Book Filter Author",
            TestDataFactory.UniqueEmail("cqrs-book-filter-author")
        );
        var bookId = await PersistBookAsync(
            "CQRS Book Filter Book",
            TestDataFactory.Isbn(),
            "Genre",
            authorId
        );
        var memberId = await PersistMemberAsync(
            "CQRS Book Filter Member",
            TestDataFactory.UniqueEmail("cqrs-book-filter-member")
        );

        var checkout = await Client.PostAsJsonAsync(
            "/api/loans/checkout",
            new CheckoutBookRequest(bookId, memberId),
            SerializerOptions
        );
        checkout.StatusCode.Should().Be(HttpStatusCode.Created);

        var response = await Client.GetAsync(
            $"/api/loans?bookId={bookId}&pageNumber=1&pageSize=10"
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var page = await response.Content.ReadFromJsonAsync<PagedResult<LoanDto>>(
            SerializerOptions
        );
        page.Should().NotBeNull();
        page!.Items.Should().NotBeEmpty();
        page.Items.Should().OnlyContain(l => l.BookId == bookId);
    }

    [Fact]
    public async Task GetLoans_via_CQRS_returns_empty_results_when_no_loans_exist()
    {
        Client.AsStaff();

        var response = await Client.GetAsync("/api/loans?pageNumber=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var page = await response.Content.ReadFromJsonAsync<PagedResult<LoanDto>>(
            SerializerOptions
        );
        page.Should().NotBeNull();
        page!.Items.Should().BeEmpty();
        page.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetLoans_via_CQRS_handles_pagination()
    {
        Client.AsStaff();

        var authorId = await PersistAuthorAsync(
            "CQRS Pagination Author",
            TestDataFactory.UniqueEmail("cqrs-pagination-author")
        );
        var memberId = await PersistMemberAsync(
            "CQRS Pagination Member",
            TestDataFactory.UniqueEmail("cqrs-pagination-member")
        );

        for (int i = 0; i < 5; i++)
        {
            var bookId = await PersistBookAsync(
                $"CQRS Book {i}",
                TestDataFactory.Isbn(),
                "Genre",
                authorId
            );
            await Client.PostAsJsonAsync(
                "/api/loans/checkout",
                new CheckoutBookRequest(bookId, memberId),
                SerializerOptions
            );
        }

        var response = await Client.GetAsync("/api/loans?pageNumber=1&pageSize=2");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var page = await response.Content.ReadFromJsonAsync<PagedResult<LoanDto>>(
            SerializerOptions
        );
        page.Should().NotBeNull();
        page!.Items.Should().HaveCount(2);
        page.TotalCount.Should().Be(5);
        page.PageNumber.Should().Be(1);
        page.PageSize.Should().Be(2);
    }
}
