using IH.LibrarySystem.Application.Loans.Dtos;
using IH.LibrarySystem.Domain.Common;
using IH.LibrarySystem.Domain.Loans;
using MediatR;

namespace IH.LibrarySystem.Application.Loans.Queries;

public sealed class GetLoansQueryHandler(ILoanRepository loanRepository)
    : IRequestHandler<GetLoansQuery, PagedResult<LoanDto>>
{
    public async Task<PagedResult<LoanDto>> Handle(
        GetLoansQuery request,
        CancellationToken cancellationToken
    )
    {
        var pagedLoans = await loanRepository.SearchAsync(
            request.Filter,
            readOnly: true,
            cancellationToken
        );

        return new PagedResult<LoanDto>(
            Items: pagedLoans.Items.Select(MapToDto).ToList(),
            TotalCount: pagedLoans.TotalCount,
            PageNumber: pagedLoans.PageNumber,
            PageSize: pagedLoans.PageSize
        );
    }

    private static LoanDto MapToDto(Loan loan) =>
        new()
        {
            Id = loan.Id,
            BookId = loan.BookId,
            MemberId = loan.MemberId,
            LoanDate = loan.LoanDate,
            DueDate = loan.DueDate,
            ReturnDate = loan.ReturnDate,
            FineAmount = loan.FineAmount,
        };
}
