using System.Text.Json;
using HHMCore.Core.Common;

namespace HHMCore.WebAPI.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (statusCode, message, logLevel) = ex switch
        {
            OperationCanceledException =>
                (499, "Request was cancelled.", LogLevel.Information),

            UnauthorizedAccessException =>
                (StatusCodes.Status401Unauthorized, "Unauthorized access.", LogLevel.Error),

            ArgumentNullException =>
                (StatusCodes.Status400BadRequest, "A required value was missing.", LogLevel.Error),

            ArgumentException =>
                (StatusCodes.Status400BadRequest, ex.Message, LogLevel.Error),

            _ =>
                (StatusCodes.Status500InternalServerError, "An unexpected error occurred.", LogLevel.Error)
        };

        // Log with the correct level — cancelled requests are not errors
        _logger.Log(
            logLevel,
            ex,
            "Exception at {Method} {Path} — {Message}",
            context.Request.Method,
            context.Request.Path,
            ex.Message);

        // Do not write a response body if the client already disconnected
        if (context.RequestAborted.IsCancellationRequested && statusCode == 499)
        {
            return;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = ApiResponse.Fail(
            _env.IsDevelopment() && statusCode == 500
                ? $"{message} Detail: {ex.Message}"
                : message);

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
