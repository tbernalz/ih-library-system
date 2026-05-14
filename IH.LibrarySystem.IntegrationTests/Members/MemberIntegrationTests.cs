using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IH.LibrarySystem.Application.Members.Dtos;
using IH.LibrarySystem.Domain.Members;
using IH.LibrarySystem.IntegrationTests.Abstractions;
using IH.LibrarySystem.IntegrationTests.Collections;
using IH.LibrarySystem.IntegrationTests.Fixtures;
using IH.LibrarySystem.IntegrationTests.Support;

namespace IH.LibrarySystem.IntegrationTests.Members;

[Collection("Integration")]
public sealed class MemberIntegrationTests : BaseIntegrationTest
{
    public MemberIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture) { }

    [Fact]
    public async Task RegisterMember_returns_created_and_persists_row()
    {
        var email = TestDataFactory.UniqueEmail("member");
        var request = new RegisterMemberRequest(TestDataFactory.PersonName(), email);

        var response = await Client.PostAsJsonAsync("/api/members", request, SerializerOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var dto = await response.Content.ReadFromJsonAsync<MemberDto>(SerializerOptions);
        dto.Should().NotBeNull();
        dto!.Email.Should().Be(email);

        var entity = await GetMemberEntityAsync(dto.Id);
        entity.Should().NotBeNull();
        entity!.Email.Should().Be(email);
        entity.Status.Should().Be(MemberStatus.Active);
    }

    [Fact]
    public async Task GetMember_returns_ok_when_member_exists()
    {
        var email = TestDataFactory.UniqueEmail("member");
        var id = await PersistMemberAsync("Existing Member", email);

        var response = await Client.GetAsync(new Uri($"/api/members/{id}", UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await response.Content.ReadFromJsonAsync<MemberDto>(SerializerOptions);
        dto.Should().NotBeNull();
        dto!.Id.Should().Be(id);
        dto.Email.Should().Be(email);
    }

    [Fact]
    public async Task GetMember_returns_404_when_missing()
    {
        var response = await Client.GetAsync(
            new Uri($"/api/members/{Guid.NewGuid()}", UriKind.Relative)
        );

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RegisterMember_returns_400_when_email_already_registered()
    {
        var email = TestDataFactory.UniqueEmail("dup");
        await PersistMemberAsync("First", email);

        var response = await Client.PostAsJsonAsync(
            "/api/members",
            new RegisterMemberRequest("Second", email),
            SerializerOptions
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateMember_updates_database_row()
    {
        var id = await PersistMemberAsync("Old Name", TestDataFactory.UniqueEmail("old"));
        var newEmail = TestDataFactory.UniqueEmail("new");

        var response = await Client.PutAsJsonAsync(
            new Uri($"/api/members/{id}", UriKind.Relative),
            new UpdateMemberRequest("New Name", newEmail),
            SerializerOptions
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var entity = await GetMemberEntityAsync(id);
        entity.Should().NotBeNull();
        entity!.Name.Should().Be("New Name");
        entity.Email.Should().Be(newEmail);
    }

    [Fact]
    public async Task PatchStatus_updates_member_status_in_database()
    {
        var id = await PersistMemberAsync("Status Member", TestDataFactory.UniqueEmail("status"));

        var response = await Client.PatchAsJsonAsync(
            new Uri($"/api/members/{id}/status", UriKind.Relative),
            new UpdateStatusRequest(MemberStatus.Suspended),
            SerializerOptions
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var entity = await GetMemberEntityAsync(id);
        entity.Should().NotBeNull();
        entity!.Status.Should().Be(MemberStatus.Suspended);
    }

    [Fact]
    public async Task DeleteMember_removes_row_from_database()
    {
        var id = await PersistMemberAsync("Delete Me", TestDataFactory.UniqueEmail("delete"));

        var response = await Client.DeleteAsync(new Uri($"/api/members/{id}", UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await MemberExistsAsync(id)).Should().BeFalse();
    }
}
