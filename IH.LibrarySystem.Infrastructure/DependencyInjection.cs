using IH.LibrarySystem.Application.Configuration;
using IH.LibrarySystem.Domain.Authors;
using IH.LibrarySystem.Domain.Books;
using IH.LibrarySystem.Domain.Loans;
using IH.LibrarySystem.Domain.Members;
using IH.LibrarySystem.Domain.SharedKernel;
using IH.LibrarySystem.Infrastructure.Data;
using IH.LibrarySystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OllamaSharp;

namespace IH.LibrarySystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string not found.");

        services.AddDbContext<LibraryDbContext>(options => options.UseSqlServer(connectionString));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IAuthorRepository, AuthorRepository>();
        services.AddScoped<IBookRepository, BookRepository>();
        services.AddScoped<IMemberRepository, MemberRepository>();
        services.AddScoped<ILoanRepository, LoanRepository>();
        services.AddScoped<LibraryDataSeeder>();

        return services.AddLibraryAiClient(configuration);
    }

    private static IServiceCollection AddLibraryAiClient(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var aiSettings =
            configuration.GetSection(AiSettings.SectionName).Get<AiSettings>()
            ?? throw new InvalidOperationException("AI configuration section is missing.");

        services.AddChatClient(builder =>
        {
            return aiSettings.Provider.ToLowerInvariant() switch
            {
                "openai" or "openrouter" => CreateOpenAiClient(aiSettings),
                "ollama" => new OllamaApiClient(new Uri(aiSettings.BaseUrl!), aiSettings.Model),
                _ => throw new NotSupportedException(
                    $"Provider {aiSettings.Provider} is not supported."
                ),
            };
        });

        return services;
    }

    private static IChatClient CreateOpenAiClient(AiSettings settings)
    {
        var options = new OpenAI.OpenAIClientOptions();
        if (!string.IsNullOrWhiteSpace(settings.BaseUrl))
        {
            options.Endpoint = new Uri(settings.BaseUrl);
        }

        return new OpenAI.Chat.ChatClient(
            settings.Model,
            new System.ClientModel.ApiKeyCredential(settings.ApiKey!),
            options
        ).AsIChatClient();
    }
}
