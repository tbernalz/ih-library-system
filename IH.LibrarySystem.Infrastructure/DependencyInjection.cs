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
using Microsoft.Extensions.Options;
using OllamaSharp;
using Pgvector.EntityFrameworkCore;
using SendGrid;

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

        services.AddDbContext<LibraryDbContext>(options =>
            options
                .UseNpgsql(connectionString, npgsql => npgsql.UseVector())
                .UseSnakeCaseNamingConvention()
        );

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        var sendGridApiKey =
            configuration["SendGrid:ApiKey"]
            ?? throw new InvalidOperationException(
                "SendGrid API Key is missing from configuration."
            );

        services.AddSingleton<ISendGridClient>(new SendGridClient(sendGridApiKey));

        services.AddScoped<IAuthorRepository, AuthorRepository>();
        services.AddScoped<IBookRepository, BookRepository>();
        services.AddScoped<IBookDiscoveryRepository, BookDiscoveryRepository>();
        services.AddScoped<IMemberRepository, MemberRepository>();
        services.AddScoped<ILoanRepository, LoanRepository>();
        services.AddScoped<LibraryDataSeeder>();

        services.AddLibraryAiClient(configuration);
        services.AddLibraryEmbeddingGenerator();

        return services;
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
                "openai" or "openrouter" => CreateOpenAiChatClient(aiSettings),
                "ollama" => new OllamaApiClient(new Uri(aiSettings.BaseUrl!), aiSettings.Model),
                _ => throw new NotSupportedException(
                    $"Provider {aiSettings.Provider} is not supported."
                ),
            };
        });

        return services;
    }

    private static IServiceCollection AddLibraryEmbeddingGenerator(this IServiceCollection services)
    {
        services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(sp =>
        {
            var aiSettings = sp.GetRequiredService<IOptions<AiSettings>>().Value;
            var discovery = sp.GetRequiredService<IOptions<DiscoverySettings>>().Value;

            if (discovery.EmbeddingDimensions != BookVectorSchema.Dimensions)
            {
                throw new InvalidOperationException(
                    $"Discovery:EmbeddingDimensions ({discovery.EmbeddingDimensions}) must equal {BookVectorSchema.Dimensions} to match the books.vector_embedding column."
                );
            }

            return CreateOpenAiCompatibleEmbeddingGenerator(aiSettings, discovery);
        });

        return services;
    }

    private static IEmbeddingGenerator<
        string,
        Embedding<float>
    > CreateOpenAiCompatibleEmbeddingGenerator(
        AiSettings aiSettings,
        DiscoverySettings discoverySettings
    )
    {
        var options = new OpenAI.OpenAIClientOptions();
        if (!string.IsNullOrWhiteSpace(aiSettings.BaseUrl))
        {
            options.Endpoint = new Uri(aiSettings.BaseUrl);
        }

        var credential = new System.ClientModel.ApiKeyCredential(aiSettings.ApiKey ?? string.Empty);
        var openAiClient = new OpenAI.OpenAIClient(credential, options);
        var embeddingClient = openAiClient.GetEmbeddingClient(discoverySettings.EmbeddingModel);

        return embeddingClient.AsIEmbeddingGenerator(discoverySettings.EmbeddingDimensions);
    }

    private static IChatClient CreateOpenAiChatClient(AiSettings settings)
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
