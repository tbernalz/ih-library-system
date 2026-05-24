namespace IH.LibrarySystem.Application.Notifications;

public interface IEmailNotificationService
{
    /// <summary>
    /// Sends an overdue loan reminder to the borrowing member.
    /// </summary>
    Task SendOverdueReminderAsync(
        OverdueReminderContext context,
        CancellationToken cancellationToken = default
    );
}

/// <summary>
/// All data needed to compose and send an overdue reminder — no domain entities cross the boundary.
/// </summary>
public sealed record OverdueReminderContext(
    Guid LoanId,
    string MemberName,
    string MemberEmail,
    string BookTitle,
    DateTime DueDate,
    int DaysOverdue
);
