using IH.LibrarySystem.Application.Configuration;
using IH.LibrarySystem.Application.Loans.Dtos;
using IH.LibrarySystem.Domain.Books;
using IH.LibrarySystem.Domain.Common;
using IH.LibrarySystem.Domain.Loans;
using IH.LibrarySystem.Domain.Members;
using IH.LibrarySystem.Domain.SharedKernel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IH.LibrarySystem.Application.Loans;

public class LoanService(
    ILoanRepository loanRepository,
    IBookRepository bookRepository,
    IMemberRepository memberRepository,
    IUnitOfWork unitOfWork,
    ILogger<LoanService> logger,
    IOptions<LibrarySettings> settings
) : ILoanService
{
    private readonly LibrarySettings _settings = settings.Value;

    public async Task<PagedResult<LoanDto>> GetLoansAsync(LoanSearchFilter filter)
    {
        logger.LogDebug("Searching loans with parameters: {@filter}", filter);

        var pagedLoans = await loanRepository.SearchAsync(filter);

        return new PagedResult<LoanDto>(
            pagedLoans.Items.Select(MapToDto).ToList(),
            pagedLoans.TotalCount,
            pagedLoans.PageNumber,
            pagedLoans.PageSize
        );
    }

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
            dueDate: DateTime.UtcNow.AddDays(_settings.DefaultLoanDurationDays)
        );

        await loanRepository.AddAsync(loan);
        await unitOfWork.SaveChangesAsync();

        return MapToDto(loan);
    }

    public async Task<LoanDto> ReturnBookAsync(Guid loanId, ReturnBookRequest request)
    {
        var loan = await loanRepository.GetWithBookAsync(loanId);

        if (loan is null)
        {
            logger.LogWarning("ReturnBookAsync failed: Loan {LoanId} not found", request.LoanId);
            throw new KeyNotFoundException($"Loan with ID {request.LoanId} not found.");
        }

        var returnDate = request.ReturnDate ?? DateTime.UtcNow;

        loan.MarkReturned(returnDate, _settings.DailyFineRate);
        loan.Book.MarkAsReturned();

        if (loan.FineAmount > 0)
        {
            logger.LogInformation(
                "Fine of {Amount} generated for Loan {Id}. Payment record pending",
                loan.FineAmount,
                loan.Id
            );
        }

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
