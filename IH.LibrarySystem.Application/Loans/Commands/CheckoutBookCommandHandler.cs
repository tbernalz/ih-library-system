using IH.LibrarySystem.Application.Common.Abstractions;
using IH.LibrarySystem.Application.Common.Exceptions;
using IH.LibrarySystem.Application.Configuration;
using IH.LibrarySystem.Application.Loans.Dtos;
using IH.LibrarySystem.Domain.Books;
using IH.LibrarySystem.Domain.Common.Exceptions;
using IH.LibrarySystem.Domain.Loans;
using IH.LibrarySystem.Domain.Members;
using IH.LibrarySystem.Domain.SharedKernel;
using MediatR;
using Microsoft.Extensions.Options;

namespace IH.LibrarySystem.Application.Loans.Commands;

public sealed class CheckoutBookCommandHandler(
    ILoanRepository loanRepository,
    IBookRepository bookRepository,
    IMemberRepository memberRepository,
    ICurrentUserContext currentUserContext,
    IUnitOfWork unitOfWork,
    IOptions<LibrarySettings> settings
) : IRequestHandler<CheckoutBookCommand, LoanDto>
{
    private readonly LibrarySettings _settings = settings.Value;

    public async Task<LoanDto> Handle(
        CheckoutBookCommand request,
        CancellationToken cancellationToken
    )
    {
        var member = await memberRepository.GetByIdAsync(
            request.MemberId,
            readOnly: true,
            cancellationToken
        );

        _ = member ?? throw new NotFoundException(nameof(Member), request.MemberId);

        EnsureCanCheckoutForMember(member);

        if (!member.CanBorrow())
        {
            throw new InvalidLoanStatusException(member.Status.ToString(), "Active");
        }

        var book = await bookRepository.GetByIdAsync(
            request.BookId,
            readOnly: false,
            cancellationToken
        );

        _ = book ?? throw new NotFoundException(nameof(Book), request.BookId);

        try
        {
            book.MarkAsLoaned();
        }
        catch (InvalidOperationException)
        {
            throw new InvalidLoanStatusException(book.Status.ToString(), "Available");
        }

        var loan = Loan.Create(
            id: Guid.NewGuid(),
            bookId: book.Id,
            memberId: member.Id,
            loanDate: DateTime.UtcNow,
            dueDate: DateTime.UtcNow.AddDays(_settings.DefaultLoanDurationDays)
        );

        await loanRepository.AddAsync(loan);
        await unitOfWork.SaveChangesAsync();

        return new LoanDto
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

    private void EnsureCanCheckoutForMember(Member member)
    {
        if (currentUserContext.IsStaffOrAdmin)
        {
            return;
        }

        var callerEmail = currentUserContext.Email;
        if (
            callerEmail is null
            || !string.Equals(member.Email, callerEmail, StringComparison.OrdinalIgnoreCase)
        )
        {
            throw new ForbiddenException(
                "You can only check out books for your own member account."
            );
        }
    }
}
