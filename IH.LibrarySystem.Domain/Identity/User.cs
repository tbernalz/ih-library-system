using IH.LibrarySystem.Domain.SharedKernel;

namespace IH.LibrarySystem.Domain.Identity;

public class User : Entity
{
    public string GoogleSubjectId { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string DisplayName { get; private set; } = default!;
    public string? AvatarUrl { get; private set; }
    public UserRole Role { get; private set; }
    public DateTime LastLoginAt { get; private set; }
    public bool IsDisabled { get; private set; }

    private User()
        : base(Guid.Empty) { }

    private User(
        Guid id,
        string googleSubjectId,
        string email,
        string displayName,
        string? avatarUrl,
        UserRole role
    )
        : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(googleSubjectId);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        GoogleSubjectId = googleSubjectId;
        Email = email;
        DisplayName = displayName;
        AvatarUrl = avatarUrl;
        Role = role;
        LastLoginAt = DateTime.UtcNow;
        IsDisabled = false;
    }

    public static User CreateFromGoogle(
        Guid id,
        string googleSubjectId,
        string email,
        string displayName,
        string? avatarUrl
    ) => new(id, googleSubjectId, email, displayName, avatarUrl, UserRole.Member);

    public void RecordGoogleLogin(string email, string displayName, string? avatarUrl)
    {
        if (IsDisabled)
            throw new InvalidOperationException($"User {Id} is disabled and cannot log in.");

        Email = email;
        DisplayName = displayName;
        AvatarUrl = avatarUrl;
        LastLoginAt = DateTime.UtcNow;
        SetUpdated();
    }
}
