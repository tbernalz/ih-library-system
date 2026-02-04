using IH.LibrarySystem.Domain.Books;
using IH.LibrarySystem.Domain.SharedKernel;

namespace IH.LibrarySystem.Domain.Loans;

public class Loan : Entity
{
    public Guid BookId { get; private set; }
    public Guid MemberId { get; private set; }

    public DateTime LoanDate { get; private set; }
    public DateTime DueDate { get; private set; }
    public DateTime? ReturnDate { get; private set; }
    public decimal? FineAmount { get; private set; }

    public Book Book { get; private set; } = default!;

    private Loan()
        : base(Guid.Empty) { }

    private Loan(Guid id, Guid bookId, Guid memberId, DateTime loanDate, DateTime dueDate)
        : base(id)
    {
        if (dueDate <= loanDate)
            throw new ArgumentException("Due date must be after loan date.");

        BookId = bookId;
        MemberId = memberId;
        LoanDate = loanDate;
        DueDate = dueDate;
    }

    public static Loan Create(
        Guid id,
        Guid bookId,
        Guid memberId,
        DateTime loanDate,
        DateTime dueDate
    ) => new(id, bookId, memberId, loanDate, dueDate);
}
