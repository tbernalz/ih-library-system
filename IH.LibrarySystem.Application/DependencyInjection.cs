using IH.LibrarySystem.Application.Ai;
using IH.LibrarySystem.Application.Authors;
using IH.LibrarySystem.Application.Books;
using IH.LibrarySystem.Application.Configuration;
using IH.LibrarySystem.Application.Discovery;
using IH.LibrarySystem.Application.Loans;
using IH.LibrarySystem.Application.Members;
using IH.LibrarySystem.Application.Notifications;
using IH.LibrarySystem.Application.Recommendations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IH.LibrarySystem.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        ArgumentNullException.ThrowIfNull(services);

        services
            .AddOptions<LibrarySettings>()
            .Bind(configuration.GetSection(LibrarySettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<AiSettings>()
            .Bind(configuration.GetSection(AiSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<DiscoverySettings>()
            .Bind(configuration.GetSection(DiscoverySettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<SendGridSettings>()
            .Bind(configuration.GetSection(SendGridSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<SeedingSettings>()
            .Bind(configuration.GetSection(SeedingSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<GoogleAuthSettings>()
            .Bind(configuration.GetSection(GoogleAuthSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<IAuthorService, AuthorService>();
        services.AddScoped<IBookService, BookService>();
        services.AddScoped<IMemberService, MemberService>();
        services.AddScoped<ILoanService, LoanService>();
        services.AddScoped<IAiService, AiService>();
        services.AddScoped<IDiscoveryService, DiscoveryService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IRecommendationService, RecommendationService>();

        services.AddHostedService<BookVectorIngestionHostedService>();
        services.AddHostedService<OverdueLoanScannerHostedService>();

        services.AddSingleton<DiscoveryIngestionQueue>();

        return services;
    }
}
