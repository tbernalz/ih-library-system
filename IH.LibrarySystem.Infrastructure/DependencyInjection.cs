using IH.LibrarySystem.Application.Common.Abstractions;
using IH.LibrarySystem.Application.Configuration;
using IH.LibrarySystem.Application.Identity;
using IH.LibrarySystem.Application.Notifications;
using IH.LibrarySystem.Domain.Authors;
using IH.LibrarySystem.Domain.Books;
using IH.LibrarySystem.Domain.Identity;
using IH.LibrarySystem.Domain.Loans;
using IH.LibrarySystem.Domain.Members;
using IH.LibrarySystem.Domain.Notifications;
using IH.LibrarySystem.Domain.SharedKernel;
using IH.LibrarySystem.Infrastructure.Data;
using IH.LibrarySystem.Infrastructure.Identity;
using IH.LibrarySystem.Infrastructure.Notifications;
using IH.LibrarySystem.Infrastructure.Repositories;
using IH.LibrarySystem.Infrastructure.VectorStore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OllamaSharp;
using SendGrid;

namespace IH.LibrarySystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddDatabase(configuration);
        services.AddVectorStore();
        services.AddRepositories();
        services.AddHttpContext();
        services.AddIdentity();
        services.AddNotifications(configuration);
        services.AddLibraryAiClient(configuration);
        services.AddLibraryEmbeddingGenerator();

        return services;
    }

    private static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is missing. "
                    + "Set it in appsettings.json or user secrets."
            );

        services.AddDbContext<LibraryDbContext>(options => options.UseSqlServer(connectionString));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<LibraryDataSeeder>();

        return services;
    }

    private static IServiceCollection AddVectorStore(this IServiceCollection services)
    {
        services.AddScoped<IQdrantVectorStore, QdrantVectorStore>();
        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IAuthorRepository, AuthorRepository>();
        services.AddScoped<IBookRepository, BookRepository>();
        services.AddScoped<IBookDiscoveryRepository, BookDiscoveryRepository>();
        services.AddScoped<IMemberRepository, MemberRepository>();
        services.AddScoped<ILoanRepository, LoanRepository>();
        services.AddScoped<INotificationLogRepository, NotificationLogRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        return services;
    }

    private static IServiceCollection AddHttpContext(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<HttpContextUserContext>();
        services.AddScoped<ICurrentUserContext>(sp =>
            sp.GetRequiredService<HttpContextUserContext>()
        );
        services.AddScoped<IClientRequestContext>(sp =>
            sp.GetRequiredService<HttpContextUserContext>()
        );

        return services;
    }

    private static IServiceCollection AddIdentity(this IServiceCollection services)
    {
        services.AddScoped<IGoogleTokenVerifier, GoogleTokenVerifier>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddSingleton<IRefreshTokenHasher, Sha256RefreshTokenHasher>();

        return services;
    }

    private static IServiceCollection AddNotifications(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var sendGridApiKey =
            configuration["SendGrid:ApiKey"]
            ?? throw new InvalidOperationException(
                "SendGrid:ApiKey is missing. Set it in user secrets or environment variables."
            );

        services.AddSingleton<ISendGridClient>(new SendGridClient(sendGridApiKey));
        services.AddScoped<IEmailNotificationService, EmailNotificationService>();

        return services;
    }

    private static IServiceCollection AddLibraryAiClient(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var aiSettings =
            configuration.GetSection(AiSettings.SectionName).Get<AiSettings>()
            ?? throw new InvalidOperationException(
                $"Configuration section '{AiSettings.SectionName}' is missing."
            );

        services.AddChatClient(_ =>
            aiSettings.Provider.ToLowerInvariant() switch
            {
                "openai" or "openrouter" => CreateOpenAiChatClient(aiSettings),
                "ollama" => new OllamaApiClient(new Uri(aiSettings.BaseUrl!), aiSettings.Model),
                _ => throw new NotSupportedException(
                    $"AI provider '{aiSettings.Provider}' is not supported. "
                        + "Supported values: openai, openrouter, ollama."
                ),
            }
        );

        return services;
    }

    private static IServiceCollection AddLibraryEmbeddingGenerator(this IServiceCollection services)
    {
        services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(sp =>
        {
            var aiSettings = sp.GetRequiredService<IOptions<AiSettings>>().Value;
            var discovery = sp.GetRequiredService<IOptions<DiscoverySettings>>().Value;

            if (discovery.EmbeddingDimensions != BookVectorSchema.Dimensions)
                throw new InvalidOperationException(
                    $"Discovery:EmbeddingDimensions ({discovery.EmbeddingDimensions}) must equal "
                        + $"{BookVectorSchema.Dimensions} to match the books.vector_embedding column."
                );

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
            options.Endpoint = new Uri(aiSettings.BaseUrl);

        var credential = new System.ClientModel.ApiKeyCredential(aiSettings.ApiKey ?? string.Empty);
        var openAiClient = new OpenAI.OpenAIClient(credential, options);
        var embeddingClient = openAiClient.GetEmbeddingClient(discoverySettings.EmbeddingModel);

        return embeddingClient.AsIEmbeddingGenerator(discoverySettings.EmbeddingDimensions);
    }

    private static IChatClient CreateOpenAiChatClient(AiSettings settings)
    {
        var options = new OpenAI.OpenAIClientOptions();
        if (!string.IsNullOrWhiteSpace(settings.BaseUrl))
            options.Endpoint = new Uri(settings.BaseUrl);

        return new OpenAI.Chat.ChatClient(
            settings.Model,
            new System.ClientModel.ApiKeyCredential(settings.ApiKey!),
            options
        ).AsIChatClient();
    }
}
