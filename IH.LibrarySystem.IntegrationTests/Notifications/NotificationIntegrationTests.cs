using FluentAssertions;
using IH.LibrarySystem.Application.Notifications;
using IH.LibrarySystem.Domain.Notifications;
using IH.LibrarySystem.IntegrationTests.Abstractions;
using IH.LibrarySystem.IntegrationTests.Collections;
using IH.LibrarySystem.IntegrationTests.Fixtures;
using IH.LibrarySystem.IntegrationTests.Stubs;
using IH.LibrarySystem.IntegrationTests.Support;
using Microsoft.Extensions.DependencyInjection;

namespace IH.LibrarySystem.IntegrationTests.Notifications;

/// <summary>
/// Integration tests for the Notifications feature.
/// Tests the NotificationService directly through the Application layer,
/// verifying interaction with the database and external service boundaries.
/// </summary>
[Collection("Integration")]
public sealed class NotificationIntegrationTests : BaseIntegrationTest
{
    public NotificationIntegrationTests(IntegrationTestFixture fixture)
        : base(fixture) { }

    [Fact]
    public async Task SendOverdueReminder_WhenLoanIsOverdue_ShouldSendNotificationAndPersistLog()
    {
        using var scope = Fixture.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var emailService = (StubEmailNotificationService)
            scope.ServiceProvider.GetRequiredService<IEmailNotificationService>();
        emailService.Clear();

        var authorId = await PersistAuthorAsync("Author", TestDataFactory.UniqueEmail("author"));
        var bookId = await PersistBookAsync("Book", TestDataFactory.Isbn(), "Fiction", authorId);
        var memberId = await PersistMemberAsync("Member", TestDataFactory.UniqueEmail("member"));
        var loanId = await PersistLoanAsync(
            bookId,
            memberId,
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow.AddDays(-5)
        );

        await service.SendOverdueReminderAsync(loanId);

        var notificationLog = await GetNotificationLogAsync(
            loanId,
            NotificationType.OverdueLoanReminder
        );
        notificationLog.Should().NotBeNull();
        notificationLog!.LoanId.Should().Be(loanId);
        notificationLog.Type.Should().Be(NotificationType.OverdueLoanReminder);
        notificationLog.SentAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        emailService.SentReminders.Should().ContainSingle();
        emailService.SentReminders[0].LoanId.Should().Be(loanId);
        emailService.SentReminders[0].DaysOverdue.Should().Be(5);
    }

    [Fact]
    public async Task SendOverdueReminder_WhenNotificationAlreadySent_ShouldSkipAndNotSendAgain()
    {
        using var scope = Fixture.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var emailService = (StubEmailNotificationService)
            scope.ServiceProvider.GetRequiredService<IEmailNotificationService>();
        emailService.Clear();

        var authorId = await PersistAuthorAsync("Author", TestDataFactory.UniqueEmail("author"));
        var bookId = await PersistBookAsync("Book", TestDataFactory.Isbn(), "Fiction", authorId);
        var memberId = await PersistMemberAsync("Member", TestDataFactory.UniqueEmail("member"));
        var loanId = await PersistLoanAsync(
            bookId,
            memberId,
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow.AddDays(-5)
        );

        await service.SendOverdueReminderAsync(loanId);

        await service.SendOverdueReminderAsync(loanId);

        emailService.SentReminders.Should().ContainSingle();
        var notificationLog = await GetNotificationLogAsync(
            loanId,
            NotificationType.OverdueLoanReminder
        );
        notificationLog.Should().NotBeNull();
    }

    [Fact]
    public async Task SendOverdueReminder_WhenLoanNotFound_ShouldThrowKeyNotFoundException()
    {
        using var scope = Fixture.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var emailService = (StubEmailNotificationService)
            scope.ServiceProvider.GetRequiredService<IEmailNotificationService>();
        emailService.Clear();

        var nonExistentLoanId = Guid.NewGuid();

        await service
            .Invoking(s => s.SendOverdueReminderAsync(nonExistentLoanId))
            .Should()
            .ThrowAsync<KeyNotFoundException>();

        emailService.SentReminders.Should().BeEmpty();
    }

    [Fact]
    public async Task SendOverdueReminder_WhenLoanIsNotOverdue_ShouldThrowInvalidOperationException()
    {
        using var scope = Fixture.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var emailService = (StubEmailNotificationService)
            scope.ServiceProvider.GetRequiredService<IEmailNotificationService>();
        emailService.Clear();

        var authorId = await PersistAuthorAsync("Author", TestDataFactory.UniqueEmail("author"));
        var bookId = await PersistBookAsync("Book", TestDataFactory.Isbn(), "Fiction", authorId);
        var memberId = await PersistMemberAsync("Member", TestDataFactory.UniqueEmail("member"));
        var loanId = await PersistLoanAsync(
            bookId,
            memberId,
            DateTime.UtcNow.AddDays(-10),
            DateTime.UtcNow.AddDays(5)
        );

        await service
            .Invoking(s => s.SendOverdueReminderAsync(loanId))
            .Should()
            .ThrowAsync<InvalidOperationException>();

        emailService.SentReminders.Should().BeEmpty();
    }

    [Fact]
    public async Task SendOverdueReminder_WhenMemberHasSeededEmailDomain_ShouldSkipNotification()
    {
        using var scope = Fixture.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var emailService = (StubEmailNotificationService)
            scope.ServiceProvider.GetRequiredService<IEmailNotificationService>();
        emailService.Clear();

        var authorId = await PersistAuthorAsync("Author", TestDataFactory.UniqueEmail("author"));
        var bookId = await PersistBookAsync("Book", TestDataFactory.Isbn(), "Fiction", authorId);
        var memberId = await PersistMemberAsync("Member", "test@seeded.ihlibrary.local");
        var loanId = await PersistLoanAsync(
            bookId,
            memberId,
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow.AddDays(-5)
        );

        await service.SendOverdueReminderAsync(loanId);

        emailService.SentReminders.Should().BeEmpty();
        var notificationLog = await GetNotificationLogAsync(
            loanId,
            NotificationType.OverdueLoanReminder
        );
        notificationLog.Should().BeNull();
    }
}
