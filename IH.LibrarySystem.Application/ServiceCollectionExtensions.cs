using IH.LibrarySystem.Application.Authors;
using IH.LibrarySystem.Application.Books;
using IH.LibrarySystem.Application.Loans;
using IH.LibrarySystem.Application.Members;
using Microsoft.Extensions.DependencyInjection;

namespace IH.LibrarySystem.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IAuthorService, AuthorService>();
        services.AddScoped<IBookService, BookService>();
        services.AddScoped<IMemberService, MemberService>();
        services.AddScoped<ILoanService, LoanService>();

        return services;
    }
}
