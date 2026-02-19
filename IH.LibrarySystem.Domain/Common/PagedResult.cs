namespace IH.LibrarySystem.Domain.Common;

public record PagedResult<T>(
    IReadOnlyCollection<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize
);
