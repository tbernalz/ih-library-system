using System.Text;
using IH.LibrarySystem.Application.Common.Security;
using IH.LibrarySystem.Application.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace IH.LibrarySystem.Api.Extensions;

public static class AuthenticationExtensions
{
    /// <summary>
    /// Registers JWT Bearer as the only authentication scheme. Tokens are issued by our own
    /// <c>AuthService</c> after Google ID token verification — Google itself is never a
    /// validated issuer for API calls, only for the initial sign-in handshake.
    /// </summary>
    public static IServiceCollection AddLibraryAuthentication(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var jwtSettings =
            configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException("Jwt configuration section is missing.");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,

                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.SigningKey)
                    ),
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context
                            .HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                            .CreateLogger("JwtBearer");
                        logger.LogDebug(
                            context.Exception,
                            "JWT authentication failed for request {Path}.",
                            context.HttpContext.Request.Path
                        );
                        return Task.CompletedTask;
                    },
                };
            });

        services
            .AddAuthorizationBuilder()
            .AddPolicy(AuthorizationPolicies.AdminOnly, p => p.RequireRole("Admin"))
            .AddPolicy(AuthorizationPolicies.StaffOrAdmin, p => p.RequireRole("Staff", "Admin"));

        return services;
    }
}
