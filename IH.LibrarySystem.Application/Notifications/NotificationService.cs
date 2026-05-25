using IH.LibrarySystem.Application.Configuration;
using IH.LibrarySystem.Domain.Loans;
using IH.LibrarySystem.Domain.Members;
using IH.LibrarySystem.Domain.Notifications;
using IH.LibrarySystem.Domain.SharedKernel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IH.LibrarySystem.Application.Notifications;

/// <summary>
/// Application service for managing and sending notifications.
/// Handles business logic for notification dispatching, idempotency, and validation.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Sends an overdue loan reminder to the borrowing member.
    /// Ensures idempotency by checking if a notification was already sent for this loan.
    /// </summary>
    Task SendOverdueReminderAsync(Guid loanId, CancellationToken cancellationToken = default);
}

public sealed class NotificationService(
    ILoanRepository loanRepository,
    IMemberRepository memberRepository,
    INotificationLogRepository notificationLogRepository,
    IEmailNotificationService emailNotificationService,
    IUnitOfWork unitOfWork,
    IOptions<SeedingSettings> seedingSettings,
    ILogger<NotificationService> logger
) : INotificationService
{
    private readonly SeedingSettings _seedingSettings = seedingSettings.Value;

    public async Task SendOverdueReminderAsync(
        Guid loanId,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogDebug("Processing overdue reminder for loan {LoanId}", loanId);

        var alreadySent = await notificationLogRepository.ExistsAsync(
            loanId,
            NotificationType.OverdueLoanReminder,
            cancellationToken
        );

        if (alreadySent)
        {
            logger.LogDebug("Notification already sent for loan {LoanId}, skipping", loanId);
            return;
        }

        var loan = await loanRepository.GetWithBookAsync(loanId);
        if (loan is null)
        {
            logger.LogWarning("Loan {LoanId} not found", loanId);
            throw new KeyNotFoundException($"Loan with ID {loanId} not found");
        }

        if (loan.DueDate >= DateTime.UtcNow.Date)
        {
            logger.LogWarning("Loan {LoanId} is not overdue", loanId);
            throw new InvalidOperationException($"Loan {loanId} is not overdue");
        }

        var member = await memberRepository.GetByIdAsync(loan.MemberId);
        if (member is null)
        {
            logger.LogWarning(
                "Member {MemberId} not found for loan {LoanId}",
                loan.MemberId,
                loanId
            );
            throw new KeyNotFoundException($"Member with ID {loan.MemberId} not found");
        }

        if (
            member.Email.EndsWith(
                _seedingSettings.SeededEmailDomain,
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            logger.LogDebug(
                "Skipping seeded member {Email} for loan {LoanId}",
                member.Email,
                loanId
            );
            return;
        }

        var daysOverdue = (DateTime.UtcNow.Date - loan.DueDate.Date).Days;

        var context = new OverdueReminderContext(
            LoanId: loan.Id,
            MemberName: member.Name,
            MemberEmail: member.Email,
            BookTitle: loan.Book.Title,
            DueDate: loan.DueDate,
            DaysOverdue: daysOverdue
        );

        await emailNotificationService.SendOverdueReminderAsync(context, cancellationToken);

        var log = NotificationLog.Create(
            Guid.NewGuid(),
            loan.Id,
            NotificationType.OverdueLoanReminder
        );

        await notificationLogRepository.AddAsync(log, cancellationToken);
        await unitOfWork.SaveChangesAsync();

        logger.LogInformation(
            "Overdue reminder sent for loan {LoanId} to member {Email} ({Days} days overdue)",
            loanId,
            member.Email,
            daysOverdue
        );
    }
}
