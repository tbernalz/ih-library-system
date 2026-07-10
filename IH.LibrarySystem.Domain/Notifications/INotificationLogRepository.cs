namespace IH.LibrarySystem.Domain.Notifications;

public interface INotificationLogRepository
{
    Task<bool> ExistsAsync(
        Guid loanId,
        NotificationType type,
        bool readOnly = false,
        CancellationToken cancellationToken = default
    );
    Task AddAsync(NotificationLog log, CancellationToken cancellationToken = default);
}
