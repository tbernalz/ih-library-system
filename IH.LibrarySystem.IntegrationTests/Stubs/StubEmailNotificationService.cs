using IH.LibrarySystem.Application.Notifications;

namespace IH.LibrarySystem.IntegrationTests.Stubs;

/// <summary>
/// Deterministic stand-in for external email providers during integration tests.
/// Captures notification contexts for verification without actually sending emails.
/// </summary>
internal sealed class StubEmailNotificationService : IEmailNotificationService
{
    public List<OverdueReminderContext> SentReminders { get; } = new();

    public Task SendOverdueReminderAsync(
        OverdueReminderContext context,
        CancellationToken cancellationToken = default
    )
    {
        SentReminders.Add(context);
        return Task.CompletedTask;
    }

    public void Clear() => SentReminders.Clear();
}
