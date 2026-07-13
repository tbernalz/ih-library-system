using FluentAssertions;
using IH.LibrarySystem.Application.Authors.Dtos;
using IH.LibrarySystem.Application.Authors.Queries;
using IH.LibrarySystem.Application.Common.Exceptions;
using IH.LibrarySystem.Domain.Authors;
using NSubstitute;
using Xunit;

namespace IH.LibrarySystem.Application.Tests.Authors.Queries;

public class GetAuthorQueryHandlerTests
{
    private readonly IAuthorRepository _authorRepository;
    private readonly GetAuthorQueryHandler _sut;

    public GetAuthorQueryHandlerTests()
    {
        _authorRepository = Substitute.For<IAuthorRepository>();
        _sut = new GetAuthorQueryHandler(_authorRepository);
    }

    [Fact]
    public async Task Handle_WhenAuthorExists_ShouldReturnAuthorDto()
    {
        var authorId = Guid.NewGuid();
        var author = Author.Create(authorId, "Test Author", "author@test.com", "Test bio");
        _authorRepository
            .GetByIdAsync(authorId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(author);

        var query = new GetAuthorQuery(authorId);
        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(authorId);
        result.Name.Should().Be("Test Author");
        result.Email.Should().Be("author@test.com");
        result.Bio.Should().Be("Test bio");
    }

    [Fact]
    public async Task Handle_WhenAuthorNotFound_ShouldThrowNotFoundException()
    {
        var authorId = Guid.NewGuid();
        _authorRepository
            .GetByIdAsync(authorId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns((Author?)null);

        var query = new GetAuthorQuery(authorId);
        var act = () => _sut.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithNullBio_ShouldReturnAuthorDtoWithNullBio()
    {
        var authorId = Guid.NewGuid();
        var author = Author.Create(authorId, "Author", "author@test.com", null);
        _authorRepository
            .GetByIdAsync(authorId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(author);

        var query = new GetAuthorQuery(authorId);
        var result = await _sut.Handle(query, CancellationToken.None);

        result.Bio.Should().BeNull();
    }
}
