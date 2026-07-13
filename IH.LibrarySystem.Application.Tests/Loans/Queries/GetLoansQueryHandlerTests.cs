using FluentAssertions;
using IH.LibrarySystem.Application.Loans.Dtos;
using IH.LibrarySystem.Application.Loans.Queries;
using IH.LibrarySystem.Domain.Common;
using IH.LibrarySystem.Domain.Loans;
using NSubstitute;
using Xunit;

namespace IH.LibrarySystem.Application.Tests.Loans.Queries;

public class GetLoansQueryHandlerTests
{
    private readonly ILoanRepository _loanRepository;
    private readonly GetLoansQueryHandler _sut;

    public GetLoansQueryHandlerTests()
    {
        _loanRepository = Substitute.For<ILoanRepository>();
        _sut = new GetLoansQueryHandler(_loanRepository);
    }

    [Fact]
    public async Task Handle_WithValidFilter_ShouldReturnPagedResult()
    {
        var filter = new LoanSearchFilter(PageNumber: 1, PageSize: 10);
        var loans = new List<Loan>
        {
            Loan.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                DateTime.UtcNow.AddDays(-10),
                DateTime.UtcNow.AddDays(4)
            ),
            Loan.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                DateTime.UtcNow.AddDays(-5),
                DateTime.UtcNow.AddDays(9)
            ),
        };
        var pagedResult = new PagedResult<Loan>(loans, 2, 1, 10);

        _loanRepository
            .SearchAsync(filter, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        var query = new GetLoansQuery(filter);
        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task Handle_WithMemberIdFilter_ShouldReturnFilteredLoans()
    {
        var memberId = Guid.NewGuid();
        var filter = new LoanSearchFilter(MemberId: memberId, PageNumber: 1, PageSize: 10);
        var loans = new List<Loan>
        {
            Loan.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                memberId,
                DateTime.UtcNow.AddDays(-10),
                DateTime.UtcNow.AddDays(4)
            ),
        };
        var pagedResult = new PagedResult<Loan>(loans, 1, 1, 10);

        _loanRepository
            .SearchAsync(filter, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        var query = new GetLoansQuery(filter);
        var result = await _sut.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items.First().MemberId.Should().Be(memberId);
    }

    [Fact]
    public async Task Handle_WithEmptyResults_ShouldReturnEmptyPagedResult()
    {
        var filter = new LoanSearchFilter(PageNumber: 1, PageSize: 10);
        var pagedResult = new PagedResult<Loan>([], 0, 1, 10);

        _loanRepository
            .SearchAsync(filter, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        var query = new GetLoansQuery(filter);
        var result = await _sut.Handle(query, CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithBookIdFilter_ShouldReturnFilteredLoans()
    {
        var bookId = Guid.NewGuid();
        var filter = new LoanSearchFilter(BookId: bookId, PageNumber: 1, PageSize: 10);
        var loans = new List<Loan>
        {
            Loan.Create(
                Guid.NewGuid(),
                bookId,
                Guid.NewGuid(),
                DateTime.UtcNow.AddDays(-10),
                DateTime.UtcNow.AddDays(4)
            ),
        };
        var pagedResult = new PagedResult<Loan>(loans, 1, 1, 10);

        _loanRepository
            .SearchAsync(filter, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        var query = new GetLoansQuery(filter);
        var result = await _sut.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items.First().BookId.Should().Be(bookId);
    }

    [Fact]
    public async Task Handle_WithIsActiveFilter_ShouldReturnFilteredLoans()
    {
        var filter = new LoanSearchFilter(IsActive: true, PageNumber: 1, PageSize: 10);
        var loans = new List<Loan>
        {
            Loan.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                DateTime.UtcNow.AddDays(-10),
                DateTime.UtcNow.AddDays(4)
            ),
        };
        var pagedResult = new PagedResult<Loan>(loans, 1, 1, 10);

        _loanRepository
            .SearchAsync(filter, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        var query = new GetLoansQuery(filter);
        var result = await _sut.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items.First().ReturnDate.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldMapLoansToDtosCorrectly()
    {
        var loanId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var loanDate = DateTime.UtcNow.AddDays(-10);
        var dueDate = DateTime.UtcNow.AddDays(4);

        var filter = new LoanSearchFilter(PageNumber: 1, PageSize: 10);
        var loans = new List<Loan> { Loan.Create(loanId, bookId, memberId, loanDate, dueDate) };
        var pagedResult = new PagedResult<Loan>(loans, 1, 1, 10);

        _loanRepository
            .SearchAsync(filter, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        var query = new GetLoansQuery(filter);
        var result = await _sut.Handle(query, CancellationToken.None);

        var dto = result.Items.First();
        dto.Id.Should().Be(loanId);
        dto.BookId.Should().Be(bookId);
        dto.MemberId.Should().Be(memberId);
        dto.LoanDate.Should().BeCloseTo(loanDate, TimeSpan.FromSeconds(1));
        dto.DueDate.Should().BeCloseTo(dueDate, TimeSpan.FromSeconds(1));
        dto.ReturnDate.Should().BeNull();
        dto.FineAmount.Should().BeNull();
    }
}
