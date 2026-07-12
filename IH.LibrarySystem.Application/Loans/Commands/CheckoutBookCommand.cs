using IH.LibrarySystem.Application.Loans.Dtos;
using MediatR;

namespace IH.LibrarySystem.Application.Loans.Commands;

public record CheckoutBookCommand(Guid BookId, Guid MemberId) : IRequest<LoanDto>;
