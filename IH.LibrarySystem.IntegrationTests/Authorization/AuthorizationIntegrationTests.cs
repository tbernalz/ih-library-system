using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IH.LibrarySystem.Application.Books.Dtos;
using IH.LibrarySystem.Application.Loans.Dtos;
using IH.LibrarySystem.Application.Members.Dtos;
using IH.LibrarySystem.IntegrationTests.Abstractions;
using IH.LibrarySystem.IntegrationTests.Auth;
using IH.LibrarySystem.IntegrationTests.Fixtures;
using IH.LibrarySystem.IntegrationTests.Support;

namespace IH.LibrarySystem.IntegrationTests.Authorization;

/// <summary>
/// Verifies the [Authorize] / role-policy wiring on the controllers, independent of each
/// feature's own business-logic tests. Three things are asserted per protected action:
///   1. An anonymous caller gets 401.
///   2. A caller with insufficient role gets 403.
///   3. A caller with sufficient role gets through to the normal (2xx/4xx-business) response.
/// </summary>
[Collection("Integration")]
public sealed class AuthorizationIntegrationTests : BaseIntegrationTest
{
    public AuthorizationIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture) { }

    #region Anonymous access (401)

    [Fact]
    public async Task GetBook_AsAnonymous_Returns404_WhenBookDoesNotExist()
    {
        using var client = Fixture.CreateClient().AsAnonymous();

        var response = await client.GetAsync(
            new Uri($"/api/books/{Guid.NewGuid()}", UriKind.Relative)
        );

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateBook_AsAnonymous_Returns401()
    {
        using var client = Fixture.CreateClient().AsAnonymous();

        var response = await client.PostAsJsonAsync(
            "/api/books",
            new CreateBookRequest("Anon Book", TestDataFactory.Isbn(), "Genre", Guid.NewGuid()),
            SerializerOptions
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SearchBooks_AsAnonymous_Returns200_BecauseCatalogIsPublic()
    {
        using var client = Fixture.CreateClient().AsAnonymous();

        var response = await client.GetAsync(
            new Uri("/api/books/search?pageNumber=1&pageSize=5", UriKind.Relative)
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetMember_AsAnonymous_Returns401()
    {
        using var client = Fixture.CreateClient().AsAnonymous();

        var response = await client.GetAsync(
            new Uri($"/api/members/{Guid.NewGuid()}", UriKind.Relative)
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CheckoutBook_AsAnonymous_Returns401()
    {
        using var client = Fixture.CreateClient().AsAnonymous();

        var response = await client.PostAsJsonAsync(
            "/api/loans/checkout",
            new CheckoutBookRequest(Guid.NewGuid(), Guid.NewGuid()),
            SerializerOptions
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Insufficient role (403)

    [Fact]
    public async Task CreateBook_AsMember_Returns403()
    {
        var authorId = await PersistAuthorAsync(
            "Forbidden Author",
            TestDataFactory.UniqueEmail("forbidden-author")
        );

        using var client = Fixture.CreateClient().AsMember();

        var response = await client.PostAsJsonAsync(
            "/api/books",
            new CreateBookRequest("Member Attempt", TestDataFactory.Isbn(), "Genre", authorId),
            SerializerOptions
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteBook_AsStaff_Returns204_StaffCanDelete()
    {
        var authorId = await PersistAuthorAsync(
            "Delete Allowed Author",
            TestDataFactory.UniqueEmail("del-allowed-author")
        );
        var bookId = await PersistBookAsync(
            "Allowed Delete",
            TestDataFactory.Isbn(),
            "Genre",
            authorId
        );

        using var client = Fixture.CreateClient().AsStaff();

        var response = await client.DeleteAsync(new Uri($"/api/books/{bookId}", UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await BookExistsAsync(bookId)).Should().BeFalse();
    }

    [Fact]
    public async Task RegisterMember_AsMember_Returns403_OnlyStaffOrAdminCanRegister()
    {
        using var client = Fixture.CreateClient().AsMember();

        var response = await client.PostAsJsonAsync(
            "/api/members",
            new RegisterMemberRequest(
                TestDataFactory.PersonName(),
                TestDataFactory.UniqueEmail("self-reg")
            ),
            SerializerOptions
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteAuthor_AsStaff_Returns204_StaffCanDelete()
    {
        var authorId = await PersistAuthorAsync(
            "Author Delete Allowed",
            TestDataFactory.UniqueEmail("author-del-allowed")
        );

        using var client = Fixture.CreateClient().AsStaff();

        var response = await client.DeleteAsync(
            new Uri($"/api/authors/{authorId}", UriKind.Relative)
        );

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await AuthorExistsAsync(authorId)).Should().BeFalse();
    }

    [Fact]
    public async Task CheckoutBook_AsMember_Returns201_MembersCanCheckout()
    {
        var authorId = await PersistAuthorAsync(
            "Checkout Allowed Author",
            TestDataFactory.UniqueEmail("checkout-allowed-author")
        );
        var bookId = await PersistBookAsync(
            "Checkout Allowed Book",
            TestDataFactory.Isbn(),
            "Genre",
            authorId
        );
        var memberId = await PersistMemberAsync(
            "Checkout Allowed Member",
            TestDataFactory.UniqueEmail("checkout-allowed-member")
        );

        using var client = Fixture.CreateClient().AsMember();

        var response = await client.PostAsJsonAsync(
            "/api/loans/checkout",
            new CheckoutBookRequest(bookId, memberId),
            SerializerOptions
        );

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    #endregion

    #region Sufficient role (passes the auth gate)

    [Fact]
    public async Task CreateBook_AsStaff_PassesAuthGate()
    {
        var authorId = await PersistAuthorAsync(
            "Allowed Author",
            TestDataFactory.UniqueEmail("allowed-author")
        );

        using var client = Fixture.CreateClient().AsStaff();

        var response = await client.PostAsJsonAsync(
            "/api/books",
            new CreateBookRequest("Staff Created", TestDataFactory.Isbn(), "Genre", authorId),
            SerializerOptions
        );

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var dto = await response.Content.ReadFromJsonAsync<BookDto>(SerializerOptions);
        dto.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteBook_AsAdmin_PassesAuthGate()
    {
        var authorId = await PersistAuthorAsync(
            "Admin Delete Author",
            TestDataFactory.UniqueEmail("admin-delete-author")
        );
        var bookId = await PersistBookAsync(
            "Admin Deletable",
            TestDataFactory.Isbn(),
            "Genre",
            authorId
        );

        using var client = Fixture.CreateClient().AsAdmin();

        var response = await client.DeleteAsync(new Uri($"/api/books/{bookId}", UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await BookExistsAsync(bookId)).Should().BeFalse();
    }

    [Fact]
    public async Task RegisterMember_AsStaff_PassesAuthGate()
    {
        using var client = Fixture.CreateClient().AsStaff();

        var response = await client.PostAsJsonAsync(
            "/api/members",
            new RegisterMemberRequest(
                TestDataFactory.PersonName(),
                TestDataFactory.UniqueEmail("staff-registered")
            ),
            SerializerOptions
        );

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var dto = await response.Content.ReadFromJsonAsync<MemberDto>(SerializerOptions);
        dto.Should().NotBeNull();
    }

    [Fact]
    public async Task GetBook_AsMember_PassesAuthGate()
    {
        var authorId = await PersistAuthorAsync(
            "Readable Author",
            TestDataFactory.UniqueEmail("readable-author")
        );
        var bookId = await PersistBookAsync(
            "Readable Book",
            TestDataFactory.Isbn(),
            "Genre",
            authorId
        );

        using var client = Fixture.CreateClient().AsMember();

        var response = await client.GetAsync(new Uri($"/api/books/{bookId}", UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Default client behavior (no headers set)

    [Fact]
    public async Task DefaultClient_WithNoAuthHeaders_BehavesAsAuthenticatedMember()
    {
        using var client = Fixture.CreateClient();

        var authorId = await PersistAuthorAsync(
            "Default Behavior Author",
            TestDataFactory.UniqueEmail("default-behavior-author")
        );
        var bookId = await PersistBookAsync(
            "Default Behavior Book",
            TestDataFactory.Isbn(),
            "Genre",
            authorId
        );

        var readResponse = await client.GetAsync(new Uri($"/api/books/{bookId}", UriKind.Relative));
        readResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var createResponse = await client.PostAsJsonAsync(
            "/api/books",
            new CreateBookRequest("Should Be Forbidden", TestDataFactory.Isbn(), "Genre", authorId),
            SerializerOptions
        );
        createResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion
}
