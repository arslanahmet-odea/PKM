using CausalCampaign.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CausalCampaign.Web.Middleware;

/// <summary>
/// Centralized Global Error Handling service implementing ASP.NET Core 10.0 IExceptionHandler.
/// Captures unhandled exceptions and returns standardized RFC 7807 ProblemDetails JSON without leaking stack traces.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException)
        {
            _logger.LogInformation("Request was cancelled by the client: {Path}", httpContext.Request.Path);
            httpContext.Response.StatusCode = 499;
            return true;
        }

        _logger.LogError(exception, "Unhandled system exception captured: {Message}", exception.Message);

        (int statusCode, string title, string errorCode, string safeDetail) = exception switch
        {
            DbUpdateConcurrencyException => (
                StatusCodes.Status409Conflict,
                "Optimistic Concurrency Conflict",
                "CONCURRENCY_CONFLICT",
                "The target record was modified concurrently by another transaction. Please retry the operation."),
            ArgumentException or FormatException => (
                StatusCodes.Status400BadRequest,
                "Invalid Request Argument",
                "INVALID_ARGUMENT",
                "One or more request parameters are invalid or improperly formatted."),
            KeyNotFoundException => (
                StatusCodes.Status404NotFound,
                "Requested Resource Not Found",
                "RESOURCE_NOT_FOUND",
                "The requested resource could not be found."),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "INTERNAL_SERVER_ERROR",
                "An unexpected internal error occurred while processing the request. Please contact the engineering team.")
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = safeDetail,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["errorCode"] = errorCode;
        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
        problemDetails.Extensions["timestampUtc"] = DateTime.UtcNow;

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            options: null,
            contentType: "application/problem+json",
            cancellationToken: cancellationToken);
        return true;
    }

    /// <summary>
    /// Maps a DomainError instance to RFC 7807 ProblemDetails and its corresponding HTTP status code.
    /// </summary>
    public static (int StatusCode, ProblemDetails Problem) MapDomainErrorToProblemDetails(
        DomainError error,
        string? requestPath = null)
    {
        int statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.AntiGamingViolation => StatusCodes.Status422UnprocessableEntity,
            ErrorType.KillSwitchActive => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status500InternalServerError
        };

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = error.Code,
            Detail = error.Message,
            Instance = requestPath
        };

        problem.Extensions["errorType"] = error.Type.ToString();
        problem.Extensions["timestampUtc"] = DateTime.UtcNow;

        return (statusCode, problem);
    }
}
