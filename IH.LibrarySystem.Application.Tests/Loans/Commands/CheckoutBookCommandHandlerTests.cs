using FluentAssertions;
using IH.LibrarySystem.Application.Common.Abstractions;
using IH.LibrarySystem.Application.Common.Exceptions;
using IH.LibrarySystem.Application.Configuration;
using IH.LibrarySystem.Application.Loans.Commands;
using IH.LibrarySystem.Application.Loans.Dtos;
using IH.LibrarySystem.Domain.Books;
using IH.LibrarySystem.Domain.Common.Exceptions;
using IH.LibrarySystem.Domain.Loans;
using IH.LibrarySystem.Domain.Members;
using IH.LibrarySystem.Domain.SharedKernel;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace IH.LibrarySystem.Application.Tests.Loans.Commands;

public class CheckoutBookCommandHandlerTests
{
    private readonly ILoanRepository _loanRepository;
    private readonly IBookRepository _bookRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOptions<LibrarySettings> _settings;
    private readonly CheckoutBookCommandHandler _sut;

    public CheckoutBookCommandHandlerTests()
    {
        _loanRepository = Substitute.For<ILoanRepository>();
        _bookRepository = Substitute.For<IBookRepository>();
        _memberRepository = Substitute.For<IMemberRepository>();
        _currentUserContext = Substitute.For<ICurrentUserContext>();
        _currentUserContext.IsStaffOrAdmin.Returns(true);
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _settings = Options.Create(
            new LibrarySettings { DailyFineRate = 0.50m, DefaultLoanDurationDays = 14 }
        );

        _sut = new CheckoutBookCommandHandler(
            _loanRepository,
            _bookRepository,
            _memberRepository,
            _currentUserContext,
            _unitOfWork,
            _settings
        );
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCreateLoan()
    {
        var bookId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var command = new CheckoutBookCommand(bookId, memberId);

        var member = Member.Create(memberId, "Borrower", "borrower@test.com");
        var book = Book.Create(bookId, "Test Book", "123456789", Guid.NewGuid(), "Fiction");

        _memberRepository
            .GetByIdAsync(memberId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(member);
        _bookRepository
            .GetByIdAsync(bookId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(book);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.BookId.Should().Be(bookId);
        result.MemberId.Should().Be(memberId);
        result.ReturnDate.Should().BeNull();
        result.DueDate.Should().BeCloseTo(DateTime.UtcNow.AddDays(14), TimeSpan.FromSeconds(1));

        await _loanRepository.Received(1).AddAsync(Arg.Any<Loan>());
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_WhenMemberNotFound_ShouldThrowNotFoundException()
    {
        var command = new CheckoutBookCommand(Guid.NewGuid(), Guid.NewGuid());
        _memberRepository
            .GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns((Member?)null);

        var act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        await _loanRepository.DidNotReceive().AddAsync(Arg.Any<Loan>());
    }

    [Fact]
    public async Task Handle_WhenMemberCannotBorrow_ShouldThrowInvalidLoanStatusException()
    {
        var memberId = Guid.NewGuid();
        var command = new CheckoutBookCommand(Guid.NewGuid(), memberId);

        var member = Member.Create(memberId, "Suspended", "suspended@test.com");
        member.UpdateStatus(MemberStatus.Suspended);

        _memberRepository
            .GetByIdAsync(memberId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(member);

        var act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidLoanStatusException>();
        await _loanRepository.DidNotReceive().AddAsync(Arg.Any<Loan>());
    }

    [Fact]
    public async Task Handle_WhenBookNotFound_ShouldThrowNotFoundException()
    {
        var bookId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var command = new CheckoutBookCommand(bookId, memberId);

        var member = Member.Create(memberId, "Borrower", "borrower@test.com");
        _memberRepository
            .GetByIdAsync(memberId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(member);
        _bookRepository
            .GetByIdAsync(bookId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns((Book?)null);

        var act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        await _loanRepository.DidNotReceive().AddAsync(Arg.Any<Loan>());
    }

    [Fact]
    public async Task Handle_WhenBookNotAvailable_ShouldThrowInvalidLoanStatusException()
    {
        var bookId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var command = new CheckoutBookCommand(bookId, memberId);

        var member = Member.Create(memberId, "Borrower", "borrower@test.com");
        var book = Book.Create(bookId, "Test Book", "123456789", Guid.NewGuid(), "Fiction");
        book.MarkAsLoaned();

        _memberRepository
            .GetByIdAsync(memberId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(member);
        _bookRepository
            .GetByIdAsync(bookId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(book);

        var act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidLoanStatusException>();
        await _loanRepository.DidNotReceive().AddAsync(Arg.Any<Loan>());
    }
}
