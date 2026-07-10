using FluentAssertions;
using IH.LibrarySystem.Application.Configuration;
using IH.LibrarySystem.Application.Notifications;
using IH.LibrarySystem.Domain.Books;
using IH.LibrarySystem.Domain.Loans;
using IH.LibrarySystem.Domain.Members;
using IH.LibrarySystem.Domain.Notifications;
using IH.LibrarySystem.Domain.SharedKernel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace IH.LibrarySystem.Application.Tests.Notifications;

public class NotificationServiceTests
{
    private readonly ILoanRepository _loanRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly INotificationLogRepository _notificationLogRepository;
    private readonly IEmailNotificationService _emailNotificationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOptions<SeedingSettings> _seedingSettings;
    private readonly ILogger<NotificationService> _logger;
    private readonly NotificationService _sut;

    public NotificationServiceTests()
    {
        _loanRepository = Substitute.For<ILoanRepository>();
        _memberRepository = Substitute.For<IMemberRepository>();
        _notificationLogRepository = Substitute.For<INotificationLogRepository>();
        _emailNotificationService = Substitute.For<IEmailNotificationService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _logger = Substitute.For<ILogger<NotificationService>>();
        _seedingSettings = Options.Create(
            new SeedingSettings { SeededEmailDomain = "@seeded.ihlibrary.local" }
        );

        _sut = new NotificationService(
            _loanRepository,
            _memberRepository,
            _notificationLogRepository,
            _emailNotificationService,
            _unitOfWork,
            _seedingSettings,
            _logger
        );
    }

    #region SendOverdueReminderAsync - Success Path Tests

    [Fact]
    public async Task SendOverdueReminderAsync_WhenValidOverdueLoan_ShouldSendNotificationAndLog()
    {
        var loanId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var memberEmail = "member@example.com";
        var memberName = "John Doe";
        var bookTitle = "Test Book Title";
        var dueDate = DateTime.UtcNow.AddDays(-5);
        var loan = CreateLoan(loanId, bookId, memberId, dueDate, bookTitle);
        var member = Member.Create(memberId, memberName, memberEmail);

        _notificationLogRepository
            .ExistsAsync(
                loanId,
                NotificationType.OverdueLoanReminder,
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(false);
        _loanRepository
            .GetWithBookAsync(loanId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(loan);
        _memberRepository
            .GetByIdAsync(memberId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(member);
        _unitOfWork.SaveChangesAsync().Returns(1);

        await _sut.SendOverdueReminderAsync(loanId);

        await _notificationLogRepository
            .Received(1)
            .ExistsAsync(
                loanId,
                NotificationType.OverdueLoanReminder,
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>()
            );
        await _loanRepository
            .Received(1)
            .GetWithBookAsync(loanId, Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _memberRepository
            .Received(1)
            .GetByIdAsync(memberId, Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _emailNotificationService
            .Received(1)
            .SendOverdueReminderAsync(
                Arg.Is<OverdueReminderContext>(ctx =>
                    ctx.LoanId == loanId
                    && ctx.MemberName == memberName
                    && ctx.MemberEmail == memberEmail
                    && ctx.BookTitle == bookTitle
                    && ctx.DaysOverdue == 5
                )
            );
        await _notificationLogRepository
            .Received(1)
            .AddAsync(
                Arg.Is<NotificationLog>(log =>
                    log.LoanId == loanId && log.Type == NotificationType.OverdueLoanReminder
                )
            );
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task SendOverdueReminderAsync_WhenLoanIsOneDayOverdue_ShouldCalculateDaysCorrectly()
    {
        var loanId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var dueDate = DateTime.UtcNow.AddDays(-1);
        var loan = CreateLoan(loanId, bookId, memberId, dueDate);
        var member = Member.Create(memberId, "Test User", "test@example.com", null);

        _notificationLogRepository
            .ExistsAsync(
                loanId,
                NotificationType.OverdueLoanReminder,
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(false);
        _loanRepository
            .GetWithBookAsync(loanId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(loan);
        _memberRepository
            .GetByIdAsync(memberId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(member);
        _unitOfWork.SaveChangesAsync().Returns(1);

        await _sut.SendOverdueReminderAsync(loanId);

        await _emailNotificationService
            .Received(1)
            .SendOverdueReminderAsync(Arg.Is<OverdueReminderContext>(ctx => ctx.DaysOverdue == 1));
    }

    [Fact]
    public async Task SendOverdueReminderAsync_WhenLoanIsThirtyDaysOverdue_ShouldCalculateDaysCorrectly()
    {
        var loanId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var dueDate = DateTime.UtcNow.AddDays(-30);
        var loan = CreateLoan(loanId, bookId, memberId, dueDate);
        var member = Member.Create(memberId, "Test User", "test@example.com", null);

        _notificationLogRepository
            .ExistsAsync(
                loanId,
                NotificationType.OverdueLoanReminder,
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(false);
        _loanRepository
            .GetWithBookAsync(loanId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(loan);
        _memberRepository
            .GetByIdAsync(memberId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(member);
        _unitOfWork.SaveChangesAsync().Returns(1);

        await _sut.SendOverdueReminderAsync(loanId);

        await _emailNotificationService
            .Received(1)
            .SendOverdueReminderAsync(Arg.Is<OverdueReminderContext>(ctx => ctx.DaysOverdue == 30));
    }

    #endregion

    #region SendOverdueReminderAsync - Idempotency Tests

    [Fact]
    public async Task SendOverdueReminderAsync_WhenNotificationAlreadySent_ShouldSkipAndNotSendAgain()
    {
        var loanId = Guid.NewGuid();

        _notificationLogRepository
            .ExistsAsync(
                loanId,
                NotificationType.OverdueLoanReminder,
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(true);

        await _sut.SendOverdueReminderAsync(loanId);

        await _loanRepository
            .DidNotReceive()
            .GetWithBookAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _memberRepository
            .DidNotReceive()
            .GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _emailNotificationService
            .DidNotReceive()
            .SendOverdueReminderAsync(Arg.Any<OverdueReminderContext>());
        await _notificationLogRepository.DidNotReceive().AddAsync(Arg.Any<NotificationLog>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }

    #endregion

    #region SendOverdueReminderAsync - Loan Not Found Tests

    [Fact]
    public async Task SendOverdueReminderAsync_WhenLoanNotFound_ShouldThrowKeyNotFoundException()
    {
        var loanId = Guid.NewGuid();

        _notificationLogRepository
            .ExistsAsync(
                loanId,
                NotificationType.OverdueLoanReminder,
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(false);
        _loanRepository
            .GetWithBookAsync(loanId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns((Loan?)null);

        var act = () => _sut.SendOverdueReminderAsync(loanId);

        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Loan with ID {loanId} not found");

        await _memberRepository
            .DidNotReceive()
            .GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _emailNotificationService
            .DidNotReceive()
            .SendOverdueReminderAsync(Arg.Any<OverdueReminderContext>());
        await _notificationLogRepository.DidNotReceive().AddAsync(Arg.Any<NotificationLog>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }

    #endregion

    #region SendOverdueReminderAsync - Loan Not Overdue Tests

    [Fact]
    public async Task SendOverdueReminderAsync_WhenLoanIsNotOverdue_ShouldThrowInvalidOperationException()
    {
        var loanId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var dueDate = DateTime.UtcNow.AddDays(5);
        var loan = CreateLoan(loanId, bookId, memberId, dueDate);

        _notificationLogRepository
            .ExistsAsync(
                loanId,
                NotificationType.OverdueLoanReminder,
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(false);
        _loanRepository
            .GetWithBookAsync(loanId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(loan);

        var act = () => _sut.SendOverdueReminderAsync(loanId);

        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage($"Loan {loanId} is not overdue");

        await _memberRepository
            .DidNotReceive()
            .GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _emailNotificationService
            .DidNotReceive()
            .SendOverdueReminderAsync(Arg.Any<OverdueReminderContext>());
        await _notificationLogRepository.DidNotReceive().AddAsync(Arg.Any<NotificationLog>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task SendOverdueReminderAsync_WhenLoanIsDueToday_ShouldThrowInvalidOperationException()
    {
        var loanId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var dueDate = DateTime.UtcNow.Date;
        var loan = CreateLoan(loanId, bookId, memberId, dueDate);

        _notificationLogRepository
            .ExistsAsync(
                loanId,
                NotificationType.OverdueLoanReminder,
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(false);
        _loanRepository
            .GetWithBookAsync(loanId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(loan);

        var act = () => _sut.SendOverdueReminderAsync(loanId);

        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage($"Loan {loanId} is not overdue");

        await _memberRepository
            .DidNotReceive()
            .GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _emailNotificationService
            .DidNotReceive()
            .SendOverdueReminderAsync(Arg.Any<OverdueReminderContext>());
        await _notificationLogRepository.DidNotReceive().AddAsync(Arg.Any<NotificationLog>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }

    #endregion

    #region SendOverdueReminderAsync - Member Not Found Tests

    [Fact]
    public async Task SendOverdueReminderAsync_WhenMemberNotFound_ShouldThrowKeyNotFoundException()
    {
        var loanId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var dueDate = DateTime.UtcNow.AddDays(-5);
        var loan = CreateLoan(loanId, bookId, memberId, dueDate);

        _notificationLogRepository
            .ExistsAsync(
                loanId,
                NotificationType.OverdueLoanReminder,
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(false);
        _loanRepository
            .GetWithBookAsync(loanId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(loan);
        _memberRepository
            .GetByIdAsync(memberId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns((Member?)null);

        var act = () => _sut.SendOverdueReminderAsync(loanId);

        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Member with ID {memberId} not found");

        await _emailNotificationService
            .DidNotReceive()
            .SendOverdueReminderAsync(Arg.Any<OverdueReminderContext>());
        await _notificationLogRepository.DidNotReceive().AddAsync(Arg.Any<NotificationLog>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }

    #endregion

    #region SendOverdueReminderAsync - Seeded Member Tests

    [Fact]
    public async Task SendOverdueReminderAsync_WhenMemberHasSeededEmailDomain_ShouldSkipNotification()
    {
        var loanId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var dueDate = DateTime.UtcNow.AddDays(-5);
        var loan = CreateLoan(loanId, bookId, memberId, dueDate);
        var member = Member.Create(memberId, "Test User", "test@seeded.ihlibrary.local", null);

        _notificationLogRepository
            .ExistsAsync(
                loanId,
                NotificationType.OverdueLoanReminder,
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(false);
        _loanRepository
            .GetWithBookAsync(loanId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(loan);
        _memberRepository
            .GetByIdAsync(memberId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(member);

        await _sut.SendOverdueReminderAsync(loanId);

        await _emailNotificationService
            .DidNotReceive()
            .SendOverdueReminderAsync(Arg.Any<OverdueReminderContext>());
        await _notificationLogRepository.DidNotReceive().AddAsync(Arg.Any<NotificationLog>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task SendOverdueReminderAsync_WhenMemberHasSeededEmailDomainWithDifferentCase_ShouldSkipNotification()
    {
        var loanId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var dueDate = DateTime.UtcNow.AddDays(-5);
        var loan = CreateLoan(loanId, bookId, memberId, dueDate);
        var member = Member.Create(memberId, "Test User", "test@SEEDED.IHLIBRARY.LOCAL", null);

        _notificationLogRepository
            .ExistsAsync(
                loanId,
                NotificationType.OverdueLoanReminder,
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(false);
        _loanRepository
            .GetWithBookAsync(loanId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(loan);
        _memberRepository
            .GetByIdAsync(memberId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(member);

        await _sut.SendOverdueReminderAsync(loanId);

        await _emailNotificationService
            .DidNotReceive()
            .SendOverdueReminderAsync(Arg.Any<OverdueReminderContext>());
        await _notificationLogRepository.DidNotReceive().AddAsync(Arg.Any<NotificationLog>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }

    #endregion

    #region SendOverdueReminderAsync - Email Service Failure Tests

    [Fact]
    public async Task SendOverdueReminderAsync_WhenEmailServiceThrows_ShouldPropagateException()
    {
        var loanId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var dueDate = DateTime.UtcNow.AddDays(-5);
        var loan = CreateLoan(loanId, bookId, memberId, dueDate);
        var member = Member.Create(memberId, "Test User", "test@example.com", null);
        var expectedException = new InvalidOperationException("SendGrid API error");

        _notificationLogRepository
            .ExistsAsync(
                loanId,
                NotificationType.OverdueLoanReminder,
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(false);
        _loanRepository
            .GetWithBookAsync(loanId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(loan);
        _memberRepository
            .GetByIdAsync(memberId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(member);
        _emailNotificationService
            .When(x => x.SendOverdueReminderAsync(Arg.Any<OverdueReminderContext>()))
            .Do(x => throw expectedException);

        var act = () => _sut.SendOverdueReminderAsync(loanId);

        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("SendGrid API error");

        await _notificationLogRepository.DidNotReceive().AddAsync(Arg.Any<NotificationLog>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }

    #endregion

    #region SendOverdueReminderAsync - Repository Failure Tests

    [Fact]
    public async Task SendOverdueReminderAsync_WhenNotificationLogRepositoryThrowsOnExists_ShouldPropagateException()
    {
        var loanId = Guid.NewGuid();
        var expectedException = new TimeoutException("Database timeout");

        _notificationLogRepository
            .When(x =>
                x.ExistsAsync(
                    loanId,
                    NotificationType.OverdueLoanReminder,
                    Arg.Any<bool>(),
                    Arg.Any<CancellationToken>()
                )
            )
            .Do(x => throw expectedException);

        var act = () => _sut.SendOverdueReminderAsync(loanId);

        await act.Should().ThrowAsync<TimeoutException>().WithMessage("Database timeout");

        await _loanRepository
            .DidNotReceive()
            .GetWithBookAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendOverdueReminderAsync_WhenLoanRepositoryThrows_ShouldPropagateException()
    {
        var loanId = Guid.NewGuid();
        var expectedException = new TimeoutException("Database timeout");

        _notificationLogRepository
            .ExistsAsync(
                loanId,
                NotificationType.OverdueLoanReminder,
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(false);
        _loanRepository
            .When(x => x.GetWithBookAsync(loanId, Arg.Any<bool>(), Arg.Any<CancellationToken>()))
            .Do(x => throw expectedException);

        var act = () => _sut.SendOverdueReminderAsync(loanId);

        await act.Should().ThrowAsync<TimeoutException>().WithMessage("Database timeout");

        await _memberRepository
            .DidNotReceive()
            .GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendOverdueReminderAsync_WhenMemberRepositoryThrows_ShouldPropagateException()
    {
        var loanId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var dueDate = DateTime.UtcNow.AddDays(-5);
        var loan = CreateLoan(loanId, bookId, memberId, dueDate);
        var expectedException = new TimeoutException("Database timeout");

        _notificationLogRepository
            .ExistsAsync(
                loanId,
                NotificationType.OverdueLoanReminder,
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(false);
        _loanRepository
            .GetWithBookAsync(loanId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(loan);
        _memberRepository
            .When(x => x.GetByIdAsync(memberId, Arg.Any<bool>(), Arg.Any<CancellationToken>()))
            .Do(x => throw expectedException);

        var act = () => _sut.SendOverdueReminderAsync(loanId);

        await act.Should().ThrowAsync<TimeoutException>().WithMessage("Database timeout");

        await _emailNotificationService
            .DidNotReceive()
            .SendOverdueReminderAsync(Arg.Any<OverdueReminderContext>());
    }

    [Fact]
    public async Task SendOverdueReminderAsync_WhenUnitOfWorkThrowsOnSave_ShouldPropagateException()
    {
        var loanId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var dueDate = DateTime.UtcNow.AddDays(-5);
        var loan = CreateLoan(loanId, bookId, memberId, dueDate);
        var member = Member.Create(memberId, "Test User", "test@example.com", null);
        var expectedException = new DbUpdateException("Concurrency conflict");

        _notificationLogRepository
            .ExistsAsync(
                loanId,
                NotificationType.OverdueLoanReminder,
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(false);
        _loanRepository
            .GetWithBookAsync(loanId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(loan);
        _memberRepository
            .GetByIdAsync(memberId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(member);
        _unitOfWork.When(x => x.SaveChangesAsync()).Do(x => throw expectedException);

        var act = () => _sut.SendOverdueReminderAsync(loanId);

        await act.Should().ThrowAsync<DbUpdateException>().WithMessage("Concurrency conflict");

        await _emailNotificationService
            .Received(1)
            .SendOverdueReminderAsync(Arg.Any<OverdueReminderContext>());
        await _notificationLogRepository.Received(1).AddAsync(Arg.Any<NotificationLog>());
    }

    #endregion

    #region SendOverdueReminderAsync - Edge Cases

    [Fact]
    public async Task SendOverdueReminderAsync_WhenMemberEmailIsEmpty_ShouldStillProcess()
    {
        var loanId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var dueDate = DateTime.UtcNow.AddDays(-5);
        var loan = CreateLoan(loanId, bookId, memberId, dueDate);
        var member = Member.Create(memberId, "Grace Lee", "grace@example.com");

        _notificationLogRepository
            .ExistsAsync(
                loanId,
                NotificationType.OverdueLoanReminder,
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(false);
        _loanRepository
            .GetWithBookAsync(loanId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(loan);
        _memberRepository
            .GetByIdAsync(memberId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(member);
        _unitOfWork.SaveChangesAsync().Returns(1);

        await _sut.SendOverdueReminderAsync(loanId);

        await _emailNotificationService
            .Received(1)
            .SendOverdueReminderAsync(
                Arg.Is<OverdueReminderContext>(ctx => ctx.MemberEmail == "grace@example.com")
            );
        await _notificationLogRepository.Received(1).AddAsync(Arg.Any<NotificationLog>());
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task SendOverdueReminderAsync_WhenBookTitleHasSpecialCharacters_ShouldHandleCorrectly()
    {
        var loanId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var dueDate = DateTime.UtcNow.AddDays(-5);
        var bookTitle = "C# & .NET: The \"Complete\" Guide (2024 Edition)";
        var loan = CreateLoan(loanId, bookId, memberId, dueDate, bookTitle);
        var member = Member.Create(memberId, "Test User", "test@example.com", null);

        _notificationLogRepository
            .ExistsAsync(
                loanId,
                NotificationType.OverdueLoanReminder,
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(false);
        _loanRepository
            .GetWithBookAsync(loanId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(loan);
        _memberRepository
            .GetByIdAsync(memberId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(member);
        _unitOfWork.SaveChangesAsync().Returns(1);

        await _sut.SendOverdueReminderAsync(loanId);

        await _emailNotificationService
            .Received(1)
            .SendOverdueReminderAsync(
                Arg.Is<OverdueReminderContext>(ctx => ctx.BookTitle == bookTitle)
            );
    }

    [Fact]
    public async Task SendOverdueReminderAsync_WhenMemberNameHasUnicodeCharacters_ShouldHandleCorrectly()
    {
        var loanId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var dueDate = DateTime.UtcNow.AddDays(-5);
        var memberName = "José García-Müller";
        var loan = CreateLoan(loanId, bookId, memberId, dueDate);
        var member = Member.Create(memberId, memberName, "test@example.com", null);

        _notificationLogRepository
            .ExistsAsync(
                loanId,
                NotificationType.OverdueLoanReminder,
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(false);
        _loanRepository
            .GetWithBookAsync(loanId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(loan);
        _memberRepository
            .GetByIdAsync(memberId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(member);
        _unitOfWork.SaveChangesAsync().Returns(1);

        await _sut.SendOverdueReminderAsync(loanId);

        await _emailNotificationService
            .Received(1)
            .SendOverdueReminderAsync(
                Arg.Is<OverdueReminderContext>(ctx => ctx.MemberName == memberName)
            );
    }

    #endregion

    #region Helper Methods

    private static Loan CreateLoan(
        Guid id,
        Guid bookId,
        Guid memberId,
        DateTime dueDate,
        string? bookTitle = null
    )
    {
        var loanDate = dueDate.AddDays(-Math.Max(30, (DateTime.UtcNow - dueDate).Days + 1));
        var loan = Loan.Create(id, bookId, memberId, loanDate, dueDate);
        var book = Book.Create(
            bookId,
            bookTitle ?? "Test Book",
            "1234567890",
            Guid.NewGuid(),
            "Fiction"
        );

        var bookProperty = typeof(Loan).GetProperty("Book");
        if (bookProperty != null)
        {
            bookProperty.SetValue(loan, book);
        }

        return loan;
    }

    #endregion
}

public class DbUpdateException : Exception
{
    public DbUpdateException(string message)
        : base(message) { }
}
