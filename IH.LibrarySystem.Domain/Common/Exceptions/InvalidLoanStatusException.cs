namespace IH.LibrarySystem.Domain.Common.Exceptions;

/// <summary>
/// Exception thrown when an operation is attempted on a loan in an invalid state.
/// This represents a violation of the loan lifecycle business rules.
/// </summary>
public class InvalidLoanStatusException : DomainException
{
    public InvalidLoanStatusException(string currentStatus, string expectedStatus)
        : base(
            $"Cannot perform operation on loan with status '{currentStatus}'. Expected status: '{expectedStatus}'."
        )
    {
        CurrentStatus = currentStatus;
        ExpectedStatus = expectedStatus;
    }

    public InvalidLoanStatusException(
        string currentStatus,
        string expectedStatus,
        Exception innerException
    )
        : base(
            $"Cannot perform operation on loan with status '{currentStatus}'. Expected status: '{expectedStatus}'.",
            innerException
        )
    {
        CurrentStatus = currentStatus;
        ExpectedStatus = expectedStatus;
    }

    public string CurrentStatus { get; }
    public string ExpectedStatus { get; }
}
