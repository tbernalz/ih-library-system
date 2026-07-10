using IH.LibrarySystem.Application.Configuration;
using IH.LibrarySystem.Domain.Loans;
using IH.LibrarySystem.Domain.Members;
using IH.LibrarySystem.Domain.Notifications;
using IH.LibrarySystem.Domain.SharedKernel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IH.LibrarySystem.Application.Notifications;

/// <summary>
/// Runs once per day. Finds all active overdue loans and sends a reminder email
/// to the borrower — exactly once per loan (idempotency via NotificationLog).
/// </summary>
public sealed class OverdueLoanScannerHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<OverdueLoanScannerHostedService> logger,
    IOptions<SeedingSettings> seedingSettings
) : BackgroundService
{
    private static readonly TimeSpan InitialDelay = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan ScanInterval = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogDebug("Overdue loan scanner started.");

        await Task.Delay(InitialDelay, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await RunScanAsync(stoppingToken);
            await Task.Delay(ScanInterval, stoppingToken);
        }
    }

    private async Task RunScanAsync(CancellationToken cancellationToken)
    {
        logger.LogDebug("Starting overdue loan scan at {UtcNow:u}", DateTime.UtcNow);

        using var scope = scopeFactory.CreateScope();
        var loanRepository = scope.ServiceProvider.GetRequiredService<ILoanRepository>();
        var memberRepository = scope.ServiceProvider.GetRequiredService<IMemberRepository>();
        var notificationLogRepository =
            scope.ServiceProvider.GetRequiredService<INotificationLogRepository>();
        var notificationService =
            scope.ServiceProvider.GetRequiredService<IEmailNotificationService>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var overdueFilter = new LoanSearchFilter(
            IsActive: true,
            IsOverdue: true,
            PageNumber: 1,
            PageSize: 200
        );

        var overdueLoans = await loanRepository.SearchAsync(
            overdueFilter,
            readOnly: true,
            cancellationToken
        );

        if (overdueLoans.TotalCount == 0)
        {
            logger.LogDebug("No overdue loans found.");
            return;
        }

        logger.LogDebug("Found {Count} overdue loan(s) to process.", overdueLoans.TotalCount);

        var notified = 0;
        var skipped = 0;

        foreach (var loan in overdueLoans.Items)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var alreadySent = await notificationLogRepository.ExistsAsync(
                    loan.Id,
                    NotificationType.OverdueLoanReminder,
                    readOnly: true,
                    cancellationToken
                );

                if (alreadySent)
                {
                    skipped++;
                    continue;
                }

                var member = await memberRepository.GetByIdAsync(
                    loan.MemberId,
                    readOnly: true,
                    cancellationToken
                );
                if (member is null)
                {
                    logger.LogWarning(
                        "Loan {LoanId} references missing member {MemberId}; skipping.",
                        loan.Id,
                        loan.MemberId
                    );
                    skipped++;
                    continue;
                }

                if (
                    member.Email.EndsWith(
                        seedingSettings.Value.SeededEmailDomain,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    logger.LogDebug(
                        "Skipping seeded member {Email} for Loan {LoanId}.",
                        member.Email,
                        loan.Id
                    );
                    skipped++;
                    continue;
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

                await notificationService.SendOverdueReminderAsync(context, cancellationToken);

                var log = NotificationLog.Create(
                    Guid.NewGuid(),
                    loan.Id,
                    NotificationType.OverdueLoanReminder
                );

                await notificationLogRepository.AddAsync(log, cancellationToken);
                await unitOfWork.SaveChangesAsync();

                notified++;

                logger.LogInformation(
                    "Overdue reminder sent for Loan {LoanId} (member: {Email}, {Days} day(s) overdue).",
                    loan.Id,
                    member.Email,
                    daysOverdue
                );
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Failed to send overdue reminder for Loan {LoanId}.", loan.Id);
            }
        }

        logger.LogDebug(
            "Overdue scan complete. Notified: {Notified}, Skipped: {Skipped}.",
            notified,
            skipped
        );
    }
}
