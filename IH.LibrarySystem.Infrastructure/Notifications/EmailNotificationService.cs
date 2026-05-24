using IH.LibrarySystem.Application.Configuration;
using IH.LibrarySystem.Application.Notifications;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace IH.LibrarySystem.Infrastructure.Notifications;

/// <summary>
/// Sends transactional emails via SendGrid's HTTP API.
/// </summary>
internal sealed class EmailNotificationService(
    ISendGridClient sendGridClient,
    IOptions<SendGridSettings> sendGridOptions,
    ILogger<EmailNotificationService> logger
) : IEmailNotificationService
{
    private readonly SendGridSettings _settings = sendGridOptions.Value;

    public async Task SendOverdueReminderAsync(
        OverdueReminderContext context,
        CancellationToken cancellationToken = default
    )
    {
        var subject = $"Reminder: \"{context.BookTitle}\" is {context.DaysOverdue} day(s) overdue";
        var body = BuildBody(context);

        if (_settings.UseConsoleSink)
        {
            logger.LogInformation(
                "[EMAIL SINK] To: {Email} | Subject: {Subject}\n{Body}",
                context.MemberEmail,
                subject,
                body
            );
            return;
        }

        var from = new EmailAddress(_settings.FromAddress, _settings.FromName);
        var to = new EmailAddress(context.MemberEmail, context.MemberName);

        var message = MailHelper.CreateSingleEmail(
            from,
            to,
            subject,
            plainTextContent: body,
            htmlContent: null
        );

        var response = await sendGridClient.SendEmailAsync(message, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Body.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"SendGrid returned failed status code {response.StatusCode}. Error details: {errorBody}"
            );
        }

        logger.LogDebug(
            "Overdue reminder successfully sent via SendGrid to {Email} for loan {LoanId}.",
            context.MemberEmail,
            context.LoanId
        );
    }

    private static string BuildBody(OverdueReminderContext ctx) =>
        $"""
            Dear {ctx.MemberName},

            This is a friendly reminder that the following book is overdue:

              Title: {ctx.BookTitle}
              Due date: {ctx.DueDate:MMMM d, yyyy}
              Overdue: {ctx.DaysOverdue} day(s)

            Please return the book as soon as possible to avoid additional fines.

            Thank you,
            IH Library System
            """;
}
