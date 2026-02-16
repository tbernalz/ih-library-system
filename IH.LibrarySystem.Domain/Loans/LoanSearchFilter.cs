using IH.LibrarySystem.Domain.Common;

namespace IH.LibrarySystem.Domain.Loans;

public record LoanSearchFilter(
    Guid? MemberId = null,
    Guid? BookId = null,
    bool? IsActive = null,
    bool? IsOverdue = null,
    int PageNumber = 1,
    int PageSize = 10
) : PagedFilter(PageNumber, PageSize);
