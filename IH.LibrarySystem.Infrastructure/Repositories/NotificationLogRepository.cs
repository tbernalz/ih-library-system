using IH.LibrarySystem.Domain.Notifications;
using IH.LibrarySystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IH.LibrarySystem.Infrastructure.Repositories;

internal sealed class NotificationLogRepository(LibraryDbContext context)
    : INotificationLogRepository
{
    public async Task<bool> ExistsAsync(
        Guid loanId,
        NotificationType type,
        bool readOnly = false,
        CancellationToken cancellationToken = default
    )
    {
        var query = context.NotificationLogs.AsQueryable();
        if (readOnly)
        {
            query = query.AsNoTracking();
        }
        return await query.AnyAsync(n => n.LoanId == loanId && n.Type == type, cancellationToken);
    }

    public async Task AddAsync(
        NotificationLog log,
        CancellationToken cancellationToken = default
    ) => await context.NotificationLogs.AddAsync(log, cancellationToken);
}
