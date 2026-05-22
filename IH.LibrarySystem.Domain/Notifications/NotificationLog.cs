using IH.LibrarySystem.Domain.SharedKernel;

namespace IH.LibrarySystem.Domain.Notifications;

public class NotificationLog : Entity
{
    public Guid LoanId { get; private set; }
    public NotificationType Type { get; private set; }
    public DateTime SentAt { get; private set; }

    private NotificationLog()
        : base(Guid.Empty) { }

    private NotificationLog(Guid id, Guid loanId, NotificationType type, DateTime sentAt)
        : base(id)
    {
        LoanId = loanId;
        Type = type;
        SentAt = sentAt;
    }

    public static NotificationLog Create(Guid id, Guid loanId, NotificationType type) =>
        new(id, loanId, type, DateTime.UtcNow);
}
