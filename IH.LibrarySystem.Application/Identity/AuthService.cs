using IH.LibrarySystem.Application.Common.Abstractions;
using IH.LibrarySystem.Application.Configuration;
using IH.LibrarySystem.Application.Identity.Dtos;
using IH.LibrarySystem.Domain.Common.Exceptions;
using IH.LibrarySystem.Domain.Identity;
using IH.LibrarySystem.Domain.Members;
using IH.LibrarySystem.Domain.SharedKernel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IH.LibrarySystem.Application.Identity;

public sealed class AuthService(
    IGoogleTokenVerifier googleTokenVerifier,
    ITokenService tokenService,
    IRefreshTokenHasher refreshTokenHasher,
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IMemberRepository memberRepository,
    ICurrentUserContext currentUserContext,
    IUnitOfWork unitOfWork,
    IOptions<JwtSettings> jwtSettings,
    ILogger<AuthService> logger
) : IAuthService
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;

    public async Task<AuthTokenResponse> LoginWithGoogleAsync(
        GoogleLoginRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default
    )
    {
        var identity = await googleTokenVerifier.VerifyAsync(request.IdToken, cancellationToken);
        if (identity is null)
        {
            logger.LogWarning("Google login rejected: ID token failed verification.");
            throw new UnauthorizedAccessException("The Google ID token is invalid or expired.");
        }

        if (!identity.EmailVerified)
        {
            logger.LogWarning(
                "Google login rejected for subject {Subject}: email not verified by Google.",
                identity.Subject
            );
            throw new UnauthorizedAccessException("Google has not verified this email address.");
        }

        var user = await userRepository.GetByGoogleSubjectIdAsync(identity.Subject);

        if (user is null)
        {
            logger.LogInformation(
                "Provisioning new user for Google subject {Subject} ({Email}).",
                identity.Subject,
                identity.Email
            );

            user = User.CreateFromGoogle(
                Guid.NewGuid(),
                identity.Subject,
                identity.Email,
                identity.Name,
                identity.Picture
            );
            await userRepository.AddAsync(user);

            if (user.Role == UserRole.Member)
            {
                var existingMember = await memberRepository.GetByEmailAsync(identity.Email);
                if (existingMember is null)
                {
                    var member = Member.Create(Guid.NewGuid(), identity.Name, identity.Email);
                    await memberRepository.AddAsync(member);
                    logger.LogInformation(
                        "Created Member entity for new user {UserId} ({Email}).",
                        user.Id,
                        identity.Email
                    );
                }
            }
        }
        else
        {
            user.RecordGoogleLogin(identity.Email, identity.Name, identity.Picture);
        }

        var tokens = await IssueTokenPairAsync(user, ipAddress);
        await unitOfWork.SaveChangesAsync();

        logger.LogInformation("User {UserId} logged in via Google.", user.Id);

        return tokens;
    }

    public async Task<AuthTokenResponse> RefreshAsync(
        RefreshTokenRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default
    )
    {
        var incomingHash = refreshTokenHasher.Hash(request.RefreshToken);
        var existing = await refreshTokenRepository.GetByTokenHashAsync(incomingHash);

        if (existing is null)
        {
            throw new InvalidRefreshTokenException("token not recognized");
        }

        if (existing.IsRevoked)
        {
            logger.LogWarning(
                "Refresh token reuse detected for user {UserId}; revoking all active sessions.",
                existing.UserId
            );
            await refreshTokenRepository.RevokeAllActiveForUserAsync(existing.UserId, ipAddress);
            await unitOfWork.SaveChangesAsync();
            throw new InvalidRefreshTokenException("token has already been used");
        }

        if (existing.IsExpired)
        {
            throw new InvalidRefreshTokenException("token has expired");
        }

        var user = await userRepository.GetByIdAsync(existing.UserId);
        if (user is null || user.IsDisabled)
        {
            throw new InvalidRefreshTokenException("associated user is missing or disabled");
        }

        var newRawRefreshToken = tokenService.GenerateRefreshToken();
        var newHash = refreshTokenHasher.Hash(newRawRefreshToken);

        existing.RevokeAndReplace(newHash, ipAddress);

        var newRefreshTokenExpiresAt = DateTime.UtcNow.AddDays(
            _jwtSettings.RefreshTokenLifetimeDays
        );
        var newRefreshTokenEntity = RefreshToken.Create(
            Guid.NewGuid(),
            user.Id,
            newHash,
            newRefreshTokenExpiresAt,
            ipAddress
        );
        await refreshTokenRepository.AddAsync(newRefreshTokenEntity);

        var accessToken = tokenService.GenerateAccessToken(user);

        await unitOfWork.SaveChangesAsync();

        return new AuthTokenResponse(
            accessToken.Token,
            accessToken.ExpiresAt,
            newRawRefreshToken,
            newRefreshTokenExpiresAt
        );
    }

    public async Task RevokeAsync(
        RevokeTokenRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default
    )
    {
        var hash = refreshTokenHasher.Hash(request.RefreshToken);
        var existing = await refreshTokenRepository.GetByTokenHashAsync(hash);

        if (existing is null || existing.IsRevoked)
        {
            return;
        }

        existing.Revoke(ipAddress);
        await unitOfWork.SaveChangesAsync();
    }

    public async Task<CurrentUserDto> GetCurrentUserAsync(
        CancellationToken cancellationToken = default
    )
    {
        var userId = currentUserContext.UserId;
        var user =
            await userRepository.GetByIdAsync(userId)
            ?? throw new Common.Exceptions.NotFoundException(nameof(User), userId);

        return new CurrentUserDto(
            user.Id,
            user.Email,
            user.DisplayName,
            user.AvatarUrl,
            user.Role.ToString()
        );
    }

    private async Task<AuthTokenResponse> IssueTokenPairAsync(User user, string? ipAddress)
    {
        var accessToken = tokenService.GenerateAccessToken(user);
        var rawRefreshToken = tokenService.GenerateRefreshToken();
        var refreshTokenHash = refreshTokenHasher.Hash(rawRefreshToken);
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenLifetimeDays);

        var refreshTokenEntity = RefreshToken.Create(
            Guid.NewGuid(),
            user.Id,
            refreshTokenHash,
            refreshTokenExpiresAt,
            ipAddress
        );

        await refreshTokenRepository.AddAsync(refreshTokenEntity);

        return new AuthTokenResponse(
            accessToken.Token,
            accessToken.ExpiresAt,
            rawRefreshToken,
            refreshTokenExpiresAt
        );
    }
}
