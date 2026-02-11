using IH.LibrarySystem.Application.Ai.Dtos;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace IH.LibrarySystem.Application.Ai;

public class AiService(IChatClient chatClient, ILogger<AiService> logger) : IAiService
{
    public async Task<string> CompleteAsync(CompleteRequest request)
    {
        logger.LogDebug(
            "Processing AI completion request with prompt length: {PromptLength}",
            request.Prompt.Length
        );

        try
        {
            var response = await chatClient.GetResponseAsync(request.Prompt);

            string responseText = response.Messages[0].Text ?? string.Empty;

            logger.LogDebug(
                "AI completion successful. Response length: {ResponseLength}",
                responseText.Length
            );

            return responseText;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during AI completion");
            throw;
        }
    }
}
