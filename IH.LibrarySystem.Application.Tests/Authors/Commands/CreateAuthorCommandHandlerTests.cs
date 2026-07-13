using FluentAssertions;
using IH.LibrarySystem.Application.Authors.Commands;
using IH.LibrarySystem.Application.Authors.Dtos;
using IH.LibrarySystem.Application.Common.Exceptions;
using IH.LibrarySystem.Domain.Authors;
using IH.LibrarySystem.Domain.SharedKernel;
using NSubstitute;
using Xunit;

namespace IH.LibrarySystem.Application.Tests.Authors.Commands;

public class CreateAuthorCommandHandlerTests
{
    private readonly IAuthorRepository _authorRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateAuthorCommandHandler _sut;

    public CreateAuthorCommandHandlerTests()
    {
        _authorRepository = Substitute.For<IAuthorRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new CreateAuthorCommandHandler(_authorRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCreateAuthor()
    {
        var command = new CreateAuthorCommand("New Author", "newauthor@test.com", "Bio");
        _authorRepository
            .GetByEmailAsync(command.Email, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns((Author?)null);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Name.Should().Be(command.Name);
        result.Email.Should().Be(command.Email);
        result.Bio.Should().Be(command.Bio);

        await _authorRepository.Received(1).AddAsync(Arg.Any<Author>());
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_WithDuplicateEmail_ShouldThrowValidationException()
    {
        var existingAuthor = Author.Create(Guid.NewGuid(), "Existing", "dup@test.com", "Bio");
        var command = new CreateAuthorCommand("New", "dup@test.com", "Bio");
        _authorRepository
            .GetByEmailAsync(command.Email, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(existingAuthor);

        var act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await _authorRepository.DidNotReceive().AddAsync(Arg.Any<Author>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_WithNullBio_ShouldCreateAuthorSuccessfully()
    {
        var command = new CreateAuthorCommand("Author", "author@test.com", null);
        _authorRepository
            .GetByEmailAsync(command.Email, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns((Author?)null);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Bio.Should().BeNull();
        await _authorRepository.Received(1).AddAsync(Arg.Any<Author>());
    }
}
