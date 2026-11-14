using GymManagement.Server.Core.DTOs.Common;
using GymManagement.Server.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Server.Api.Middleware;

public sealed class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            if (context.Response.HasStarted)
            {
                _logger.LogError(exception, "An exception occurred after the response started.");
                throw;
            }

            await WriteErrorResponseAsync(context, exception);
        }
    }

    private async Task WriteErrorResponseAsync(HttpContext context, Exception exception)
    {
        string unexpectedMessage = "An unexpected error occurred.";
        if (_environment.IsDevelopment())
        {
            unexpectedMessage = exception.Message;
        }

        var (statusCode, errorCode, message) = exception switch
        {
            ValidationException => (
                StatusCodes.Status400BadRequest,
                "VALIDATION_ERROR",
                exception.Message),
            NotFoundException => (
                StatusCodes.Status404NotFound,
                "NOT_FOUND",
                exception.Message),
            BusinessRuleException => (
                StatusCodes.Status409Conflict,
                "BUSINESS_RULE_VIOLATION",
                exception.Message),
            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                "UNAUTHORIZED",
                "Authentication is required."),
            _ => (
                StatusCodes.Status500InternalServerError,
                "INTERNAL_SERVER_ERROR",
                unexpectedMessage)
        };

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "An unhandled exception occurred while processing the request.");
        }
        else
        {
            _logger.LogWarning(exception, "A request failed with error code {ErrorCode}.", errorCode);
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(
            new ApiErrorResponse(false, null, errorCode, message),
            context.RequestAborted);
    }
}
