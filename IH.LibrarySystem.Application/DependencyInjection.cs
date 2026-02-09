using IH.LibrarySystem.Application.Ai;
using IH.LibrarySystem.Application.Authors;
using IH.LibrarySystem.Application.Books;
using IH.LibrarySystem.Application.Configuration;
using IH.LibrarySystem.Application.Loans;
using IH.LibrarySystem.Application.Members;
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

        services.AddScoped<IAuthorService, AuthorService>();
        services.AddScoped<IBookService, BookService>();
        services.AddScoped<IMemberService, MemberService>();
        services.AddScoped<ILoanService, LoanService>();

        return services;
    }
}
