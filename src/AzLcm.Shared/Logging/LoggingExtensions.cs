using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AzLcm.Shared.Logging;

public static class LoggingExtensions
{
    public static void LogOperationStart(this ILogger logger, string operationName, object? parameters = null)
    {
        logger.LogInformation("Starting operation: {OperationName} with parameters: {@Parameters}", 
            operationName, parameters);
    }

    public static void LogOperationSuccess(this ILogger logger, string operationName, TimeSpan duration, object? result = null)
    {
        logger.LogInformation("Operation completed successfully: {OperationName} in {Duration}ms. Result: {@Result}", 
            operationName, duration.TotalMilliseconds, result);
    }

    public static void LogOperationFailure(this ILogger logger, string operationName, Exception exception, object? parameters = null)
    {
        logger.LogError(exception, "Operation failed: {OperationName} with parameters: {@Parameters}", 
            operationName, parameters);
    }

    public static void LogServiceHealthEvent(this ILogger logger, string eventType, string? serviceName = null, object? details = null)
    {
        logger.LogInformation("Service health event: {EventType} for service: {ServiceName}. Details: {@Details}", 
            eventType, serviceName, details);
    }

    public static void LogPolicyEvent(this ILogger logger, string policyId, string changeKind, object? details = null)
    {
        logger.LogInformation("Policy event: {PolicyId} with change: {ChangeKind}. Details: {@Details}", 
            policyId, changeKind, details);
    }

    public static void LogFeedEvent(this ILogger logger, string feedTitle, string? verdict = null, object? details = null)
    {
        logger.LogInformation("Feed event: {FeedTitle} with verdict: {Verdict}. Details: {@Details}", 
            feedTitle, verdict, details);
    }

    public static void LogStorageOperation(this ILogger logger, string operation, string? tableName = null, object? key = null)
    {
        logger.LogDebug("Storage operation: {Operation} on table: {TableName} with key: {@Key}", 
            operation, tableName, key);
    }

    public static void LogExternalServiceCall(this ILogger logger, string serviceName, string endpoint, object? parameters = null)
    {
        logger.LogDebug("External service call: {ServiceName} at {Endpoint} with parameters: {@Parameters}", 
            serviceName, endpoint, parameters);
    }

    public static void LogCriticalError(this ILogger logger, Exception exception, string context, object? additionalData = null)
    {
        logger.LogCritical(exception, "Critical error in {Context}. Additional data: {@AdditionalData}", 
            context, additionalData);
    }

    public static void LogSecurityEvent(this ILogger logger, string eventType, string? details = null)
    {
        logger.LogWarning("Security event: {EventType}. Details: {Details}", eventType, details);
    }

    public static void LogConfigurationEvent(this ILogger logger, string configType, string action, object? details = null)
    {
        logger.LogInformation("Configuration event: {ConfigType} - {Action}. Details: {@Details}", 
            configType, action, details);
    }

    public static IDisposable? BeginOperationScope(this ILogger logger, string operationName, object? parameters = null)
    {
        return logger.BeginScope(new Dictionary<string, object?>
        {
            ["OperationName"] = operationName,
            ["Parameters"] = parameters,
            ["OperationId"] = Guid.NewGuid().ToString()
        });
    }
}