using IH.LibrarySystem.Application.Configuration;
using IH.LibrarySystem.Application.Discovery.Helpers;
using IH.LibrarySystem.Domain.Books;
using IH.LibrarySystem.Domain.SharedKernel;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IH.LibrarySystem.Application.Discovery;

/// <summary>
/// Indexes books missing vector embeddings using the configured embedding model.
/// </summary>
public sealed class BookVectorIngestionHostedService(
    IServiceScopeFactory scopeFactory,
    IOptions<DiscoverySettings> discoveryOptions,
    ILogger<BookVectorIngestionHostedService> logger,
    DiscoveryIngestionQueue discoveryIngestionQueue
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = discoveryOptions.Value;
        if (!settings.EnableBackgroundIngestion)
        {
            logger.LogInformation("Discovery background ingestion is disabled.");
            return;
        }

        await RunIngestionAsync(settings, stoppingToken);

        try
        {
            await foreach (var _ in discoveryIngestionQueue.Reader.WithCancellation(stoppingToken))
            {
                logger.LogInformation("Signal received: Processing new book embeddings.");
                await RunIngestionAsync(settings, stoppingToken);
            }
        }
        catch (OperationCanceledException) { }
    }

    private async Task RunIngestionAsync(
        DiscoverySettings settings,
        CancellationToken cancellationToken
    )
    {
        using var scope = scopeFactory.CreateScope();
        var embeddingGenerator = scope.ServiceProvider.GetService<
            IEmbeddingGenerator<string, Embedding<float>>
        >();
        if (embeddingGenerator is null)
        {
            logger.LogWarning(
                "No embedding generator is registered; skipping discovery ingestion (e.g. unsupported AI provider)."
            );
            return;
        }

        var discoveryRepository =
            scope.ServiceProvider.GetRequiredService<IBookDiscoveryRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        while (!cancellationToken.IsCancellationRequested)
        {
            var pending = await discoveryRepository.GetBooksMissingEmbeddingsAsync(
                cancellationToken
            );
            if (pending.Count == 0)
            {
                logger.LogInformation("Discovery ingestion: all books already have embeddings.");
                return;
            }

            logger.LogInformation(
                "Discovery ingestion: processing {Count} books without embeddings.",
                pending.Count
            );

            foreach (var batch in pending.Chunk(settings.IngestionBatchSize))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var texts = batch.Select(BookEmbeddingText.Format).ToList();
                var generated = await embeddingGenerator.GenerateAsync(
                    texts,
                    cancellationToken: cancellationToken
                );
                if (generated.Count != batch.Length)
                {
                    logger.LogWarning(
                        "Embedding batch size mismatch (expected {Expected}, got {Actual}).",
                        batch.Length,
                        generated.Count
                    );
                }

                for (var i = 0; i < Math.Min(batch.Length, generated.Count); i++)
                {
                    var vec = generated[i].Vector;
                    if (vec.IsEmpty)
                    {
                        logger.LogWarning(
                            "Empty embedding for book {BookId}; skipping.",
                            batch[i].Id
                        );
                        continue;
                    }

                    await discoveryRepository.UpdateEmbeddingAsync(
                        batch[i].Id,
                        vec.ToArray(),
                        cancellationToken
                    );
                }

                await unitOfWork.SaveChangesAsync();
            }
        }
    }
}
