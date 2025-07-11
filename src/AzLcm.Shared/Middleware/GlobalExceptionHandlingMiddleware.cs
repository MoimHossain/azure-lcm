using AzLcm.Shared.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace AzLcm.Shared.Middleware;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger, JsonSerializerOptions jsonOptions)
    {
        _next = next;
        _logger = logger;
        _jsonOptions = jsonOptions;
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

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var errorResponse = new ErrorResponse();
        
        switch (exception)
        {
            case ArgumentNullException:
            case ArgumentException:
                errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Message = "Invalid request parameters";
                errorResponse.Detail = exception.Message;
                _logger.LogWarning(exception, "Bad request: {Message}", exception.Message);
                break;
                
            case UnauthorizedAccessException:
                errorResponse.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse.Message = "Unauthorized access";
                errorResponse.Detail = "Authentication required";
                _logger.LogWarning(exception, "Unauthorized access attempt");
                break;
                
            case KeyNotFoundException:
                errorResponse.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.Message = "Resource not found";
                errorResponse.Detail = exception.Message;
                _logger.LogWarning(exception, "Resource not found: {Message}", exception.Message);
                break;
                
            case TimeoutException:
                errorResponse.StatusCode = (int)HttpStatusCode.RequestTimeout;
                errorResponse.Message = "Request timeout";
                errorResponse.Detail = "The request took too long to complete";
                _logger.LogWarning(exception, "Request timeout: {Message}", exception.Message);
                break;
                
            case TaskCanceledException when ((TaskCanceledException)exception).CancellationToken.IsCancellationRequested:
                errorResponse.StatusCode = (int)HttpStatusCode.RequestTimeout;
                errorResponse.Message = "Request was cancelled";
                errorResponse.Detail = "The request was cancelled before completion";
                _logger.LogWarning(exception, "Request cancelled: {Message}", exception.Message);
                break;
                
            case InvalidOperationException:
                errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Message = "Invalid operation";
                errorResponse.Detail = exception.Message;
                _logger.LogWarning(exception, "Invalid operation: {Message}", exception.Message);
                break;
                
            default:
                errorResponse.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.Message = "An error occurred while processing the request";
                errorResponse.Detail = "Please try again later";
                _logger.LogCriticalError(exception, "Unhandled exception in request pipeline", 
                    new { Path = context.Request.Path, Method = context.Request.Method });
                break;
        }

        var response = context.Response;
        response.ContentType = "application/json";
        response.StatusCode = errorResponse.StatusCode;

        var jsonResponse = JsonSerializer.Serialize(errorResponse, _jsonOptions);
        await response.WriteAsync(jsonResponse);
    }
}

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public string TraceId { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public static class GlobalExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
    }
}