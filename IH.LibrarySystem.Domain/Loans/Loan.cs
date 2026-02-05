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

    public void MarkReturned(DateTime returnedAt, decimal dailyRate)
    {
        if (ReturnDate.HasValue)
            throw new InvalidOperationException($"Book already returned on {ReturnDate:u}");

        if (returnedAt < LoanDate)
            throw new ArgumentException("Return date cannot be before loan date.");

        ReturnDate = returnedAt;

        FineAmount = CalculateFine(dailyRate);

        SetUpdated();
    }

    public decimal CalculateFine(decimal dailyRate)
    {
        if (dailyRate < 0)
            throw new ArgumentOutOfRangeException(nameof(dailyRate));

        var current = ReturnDate ?? DateTime.UtcNow;
        if (current.Date <= DueDate.Date)
            return 0m;

        var overdueDays = (current.Date - DueDate.Date).Days;
        if (overdueDays <= 0)
            return 0m;

        return overdueDays * dailyRate;
    }
}
