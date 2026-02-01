using IH.LibrarySystem.Application.Loans.Dtos;
using IH.LibrarySystem.Domain.Books;
using IH.LibrarySystem.Domain.Loans;
using IH.LibrarySystem.Domain.Members;
using IH.LibrarySystem.Domain.SharedKernel;
using Microsoft.Extensions.Logging;

namespace IH.LibrarySystem.Application.Loans;

public class LoanService(
    ILoanRepository loanRepository,
    ILogger<LoanService> logger,
    IBookRepository bookRepository,
    IMemberRepository memberRepository,
    IUnitOfWork unitOfWork
) : ILoanService
{
    private const int DefaultLoanDurationDays = 14;

    public async Task<LoanDto> GetLoanByIdAsync(Guid loanId)
    {
        logger.LogDebug("Fetching loan with {LoanId}", loanId);

        var loan = await loanRepository.GetByIdAsync(loanId);

        if (loan is null)
        {
            logger.LogWarning("Loan retrieval failed: ID {LoanId} not found", loanId);
            throw new KeyNotFoundException($"Loan with ID {loanId} not found.");
        }

        return MapToDto(loan);
    }

    public async Task<LoanDto> CheckoutBookAsync(CheckoutBookRequest request)
    {
        logger.LogInformation(
            "Initiating book checkout: {BookId} for member {MemberId}",
            request.BookId,
            request.MemberId
        );

        var member = await memberRepository.GetByIdAsync(request.MemberId);

        if (member == null)
        {
            logger.LogWarning("Member retrieval failed: ID {MemberId} not found", request.MemberId);
            throw new KeyNotFoundException($"Member with ID {request.MemberId} not found.");
        }

        var book = await bookRepository.GetByIdAsync(request.BookId);
        if (book == null)
        {
            logger.LogWarning("Book retrieval failed: ID {BookId} not found", request.BookId);
            throw new KeyNotFoundException($"Book with ID {request.BookId} not found.");
        }
        if (!member.CanBorrow())
        {
            logger.LogWarning(
                "Member {MemberName} is not able to borrow (Member status: {MemberStatus})",
                member.Name,
                member.Status
            );
            throw new InvalidOperationException(
                $"Member {member.Name} is not able to borrow (Member status: {member.Status})."
            );
        }

        try
        {
            book.MarkAsLoaned();
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(
                "Book '{BookTitle}' is not available (Book status: {BookStatus})",
                book.Title,
                book.Status
            );
            throw;
        }

        var loan = Loan.Create(
            id: Guid.NewGuid(),
            bookId: book.Id,
            memberId: member.Id,
            loanDate: DateTime.UtcNow,
            dueDate: DateTime.UtcNow.AddDays(DefaultLoanDurationDays)
        );

        await loanRepository.AddAsync(loan);
        await unitOfWork.SaveChangesAsync();

        return MapToDto(loan);
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
