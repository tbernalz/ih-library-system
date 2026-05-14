using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IH.LibrarySystem.Application.Loans.Dtos;
using IH.LibrarySystem.Domain.Books;
using IH.LibrarySystem.Domain.Common;
using IH.LibrarySystem.IntegrationTests.Abstractions;
using IH.LibrarySystem.IntegrationTests.Collections;
using IH.LibrarySystem.IntegrationTests.Fixtures;
using IH.LibrarySystem.IntegrationTests.Support;

namespace IH.LibrarySystem.IntegrationTests.Loans;

[Collection("Integration")]
public sealed class LoanIntegrationTests : BaseIntegrationTest
{
    public LoanIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture) { }

    [Fact]
    public async Task CheckoutBook_creates_loan_and_marks_book_as_loaned()
    {
        var authorId = await PersistAuthorAsync(
            "Loan Author",
            TestDataFactory.UniqueEmail("loan-author")
        );
        var bookId = await PersistBookAsync("Loanable", TestDataFactory.Isbn(), "Genre", authorId);
        var memberId = await PersistMemberAsync(
            "Borrower",
            TestDataFactory.UniqueEmail("borrower")
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
    public async Task CheckoutBook_returns_400_when_book_is_not_available()
    {
        var authorId = await PersistAuthorAsync(
            "Busy Author",
            TestDataFactory.UniqueEmail("busy-author")
        );
        var bookId = await PersistBookAsync("Busy Book", TestDataFactory.Isbn(), "Genre", authorId);
        var memberA = await PersistMemberAsync("Member A", TestDataFactory.UniqueEmail("member-a"));
        var memberB = await PersistMemberAsync("Member B", TestDataFactory.UniqueEmail("member-b"));

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
    public async Task GetLoans_can_filter_by_member_id()
    {
        var authorId = await PersistAuthorAsync(
            "Filter Author",
            TestDataFactory.UniqueEmail("filter-author")
        );
        var bookId = await PersistBookAsync(
            "Filter Book",
            TestDataFactory.Isbn(),
            "Genre",
            authorId
        );
        var memberId = await PersistMemberAsync(
            "Filter Member",
            TestDataFactory.UniqueEmail("filter-member")
        );

        var checkout = await Client.PostAsJsonAsync(
            "/api/loans/checkout",
            new CheckoutBookRequest(bookId, memberId),
            SerializerOptions
        );
        checkout.StatusCode.Should().Be(HttpStatusCode.Created);

        var response = await Client.GetAsync(
            new Uri($"/api/loans?memberId={memberId}&pageNumber=1&pageSize=10", UriKind.Relative)
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
    public async Task ReturnBook_sets_return_date_in_database()
    {
        var authorId = await PersistAuthorAsync(
            "Return Author",
            TestDataFactory.UniqueEmail("return-author")
        );
        var bookId = await PersistBookAsync(
            "Return Book",
            TestDataFactory.Isbn(),
            "Genre",
            authorId
        );
        var memberId = await PersistMemberAsync(
            "Return Member",
            TestDataFactory.UniqueEmail("return-member")
        );

        var checkout = await Client.PostAsJsonAsync(
            "/api/loans/checkout",
            new CheckoutBookRequest(bookId, memberId),
            SerializerOptions
        );
        var loan = await checkout.Content.ReadFromJsonAsync<LoanDto>(SerializerOptions);
        loan.Should().NotBeNull();

        var returnDate = DateTime.UtcNow.AddMinutes(5);
        var response = await Client.PostAsJsonAsync(
            new Uri($"/api/loans/return/{loan!.Id}", UriKind.Relative),
            new ReturnBookRequest(loan.Id, returnDate),
            SerializerOptions
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var persisted = await GetLoanEntityAsync(loan.Id);
        persisted.Should().NotBeNull();
        persisted!.ReturnDate.Should().NotBeNull();
        persisted.ReturnDate!.Value.Should().BeCloseTo(returnDate, TimeSpan.FromSeconds(1));

        var book = await GetBookEntityAsync(bookId);
        book!.Status.Should().Be(BookStatus.Available);
    }

    [Fact]
    public async Task DeleteLoan_returns_400_when_loan_is_still_active()
    {
        var authorId = await PersistAuthorAsync(
            "Active Author",
            TestDataFactory.UniqueEmail("active-author")
        );
        var bookId = await PersistBookAsync(
            "Active Book",
            TestDataFactory.Isbn(),
            "Genre",
            authorId
        );
        var memberId = await PersistMemberAsync(
            "Active Member",
            TestDataFactory.UniqueEmail("active-member")
        );

        var checkout = await Client.PostAsJsonAsync(
            "/api/loans/checkout",
            new CheckoutBookRequest(bookId, memberId),
            SerializerOptions
        );
        var loan = await checkout.Content.ReadFromJsonAsync<LoanDto>(SerializerOptions);
        loan.Should().NotBeNull();

        var response = await Client.DeleteAsync(
            new Uri($"/api/loans/{loan!.Id}", UriKind.Relative)
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        (await LoanExistsAsync(loan.Id)).Should().BeTrue();
    }

    [Fact]
    public async Task DeleteLoan_returns_204_after_book_is_returned()
    {
        var authorId = await PersistAuthorAsync(
            "Done Author",
            TestDataFactory.UniqueEmail("done-author")
        );
        var bookId = await PersistBookAsync("Done Book", TestDataFactory.Isbn(), "Genre", authorId);
        var memberId = await PersistMemberAsync(
            "Done Member",
            TestDataFactory.UniqueEmail("done-member")
        );

        var checkout = await Client.PostAsJsonAsync(
            "/api/loans/checkout",
            new CheckoutBookRequest(bookId, memberId),
            SerializerOptions
        );
        var loan = await checkout.Content.ReadFromJsonAsync<LoanDto>(SerializerOptions);
        loan.Should().NotBeNull();

        var returned = await Client.PostAsJsonAsync(
            new Uri($"/api/loans/return/{loan!.Id}", UriKind.Relative),
            new ReturnBookRequest(loan.Id, DateTime.UtcNow),
            SerializerOptions
        );
        returned.StatusCode.Should().Be(HttpStatusCode.OK);

        var delete = await Client.DeleteAsync(new Uri($"/api/loans/{loan.Id}", UriKind.Relative));

        delete.StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await LoanExistsAsync(loan.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task GetLoan_returns_404_when_missing()
    {
        var response = await Client.GetAsync(
            new Uri($"/api/loans/{Guid.NewGuid()}", UriKind.Relative)
        );

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
