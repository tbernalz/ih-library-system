using IH.LibrarySystem.Application.Loans.Dtos;
using IH.LibrarySystem.Domain.Common;
using IH.LibrarySystem.Domain.Loans;
using MediatR;

namespace IH.LibrarySystem.Application.Loans.Queries;

public record GetLoansQuery(LoanSearchFilter Filter) : IRequest<PagedResult<LoanDto>>;
