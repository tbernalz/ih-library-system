using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace IH.LibrarySystem.Api.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);

        var (statusCode, title) = exception switch
        {
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource Not Found"),
            ArgumentException or InvalidOperationException => (
                StatusCodes.Status400BadRequest,
                "Invalid Request"
            ),
            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                "Unauthorized Access"
            ),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred"),
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,

            Detail =
                statusCode == StatusCodes.Status500InternalServerError
                    ? "Please contact support."
                    : exception.Message,
            Instance = httpContext.Request.Path,
            Extensions = new Dictionary<string, object?>
            {
                { "traceId", httpContext.TraceIdentifier },
            },
        };

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
