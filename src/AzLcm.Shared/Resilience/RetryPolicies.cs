using AzLcm.Shared.Logging;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;

namespace AzLcm.Shared.Resilience;

public static class RetryPolicies
{
    public static async Task<T> ExecuteWithRetryAsync<T>(
        Func<Task<T>> operation,
        ILogger logger,
        string operationName,
        int maxRetries = 3,
        TimeSpan? baseDelay = null,
        Func<Exception, bool>? shouldRetry = null)
    {
        var delay = baseDelay ?? TimeSpan.FromSeconds(1);
        var attempt = 0;
        
        while (true)
        {
            try
            {
                attempt++;
                logger.LogDebug("Executing {OperationName}, attempt {Attempt}/{MaxRetries}", 
                    operationName, attempt, maxRetries);
                
                return await operation();
            }
            catch (Exception ex) when (attempt < maxRetries && ShouldRetry(ex, shouldRetry))
            {
                var waitTime = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * Math.Pow(2, attempt - 1));
                
                logger.LogWarning(ex, "Operation {OperationName} failed on attempt {Attempt}/{MaxRetries}. " +
                                     "Retrying in {WaitTime}ms", 
                    operationName, attempt, maxRetries, waitTime.TotalMilliseconds);
                
                await Task.Delay(waitTime);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Operation {OperationName} failed after {Attempt} attempts", 
                    operationName, attempt);
                throw;
            }
        }
    }

    public static async Task ExecuteWithRetryAsync(
        Func<Task> operation,
        ILogger logger,
        string operationName,
        int maxRetries = 3,
        TimeSpan? baseDelay = null,
        Func<Exception, bool>? shouldRetry = null)
    {
        await ExecuteWithRetryAsync(async () =>
        {
            await operation();
            return true;
        }, logger, operationName, maxRetries, baseDelay, shouldRetry);
    }

    private static bool ShouldRetry(Exception exception, Func<Exception, bool>? customShouldRetry)
    {
        if (customShouldRetry != null)
        {
            return customShouldRetry(exception);
        }

        return exception switch
        {
            HttpRequestException httpEx => IsTransientHttpError(httpEx),
            TaskCanceledException => true,
            TimeoutException => true,
            SocketException => true,
            _ when exception.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase) => true,
            _ when exception.Message.Contains("network", StringComparison.OrdinalIgnoreCase) => true,
            _ when exception.Message.Contains("connection", StringComparison.OrdinalIgnoreCase) => true,
            _ => false
        };
    }

    private static bool IsTransientHttpError(HttpRequestException httpEx)
    {
        var message = httpEx.Message.ToLowerInvariant();
        
        // Check for specific HTTP status codes that indicate transient errors
        if (message.Contains("500") || message.Contains("502") || message.Contains("503") || 
            message.Contains("504") || message.Contains("408") || message.Contains("429"))
        {
            return true;
        }

        // Check for network-related errors
        return message.Contains("timeout") || 
               message.Contains("connection") || 
               message.Contains("network") ||
               message.Contains("dns");
    }
}

public class CircuitBreakerPolicy
{
    private readonly object _lock = new();
    private readonly int _failureThreshold;
    private readonly TimeSpan _timeout;
    private readonly ILogger _logger;
    private int _consecutiveFailures;
    private DateTime _lastFailureTime;
    private CircuitState _state = CircuitState.Closed;

    public CircuitBreakerPolicy(int failureThreshold, TimeSpan timeout, ILogger logger)
    {
        _failureThreshold = failureThreshold;
        _timeout = timeout;
        _logger = logger;
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, string operationName)
    {
        if (_state == CircuitState.Open)
        {
            if (DateTime.UtcNow - _lastFailureTime < _timeout)
            {
                _logger.LogWarning("Circuit breaker is OPEN for {OperationName}. Rejecting request.", operationName);
                throw new CircuitBreakerOpenException($"Circuit breaker is open for {operationName}");
            }
            
            _state = CircuitState.HalfOpen;
            _logger.LogInformation("Circuit breaker transitioning to HALF-OPEN for {OperationName}", operationName);
        }

        try
        {
            var result = await operation();
            
            if (_state == CircuitState.HalfOpen)
            {
                Reset(operationName);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            RecordFailure(operationName, ex);
            throw;
        }
    }

    private void RecordFailure(string operationName, Exception exception)
    {
        lock (_lock)
        {
            _consecutiveFailures++;
            _lastFailureTime = DateTime.UtcNow;

            if (_consecutiveFailures >= _failureThreshold)
            {
                _state = CircuitState.Open;
                _logger.LogError(exception, "Circuit breaker OPENED for {OperationName} after {FailureCount} consecutive failures", 
                    operationName, _consecutiveFailures);
            }
            else
            {
                _logger.LogWarning(exception, "Circuit breaker recorded failure {FailureCount}/{FailureThreshold} for {OperationName}", 
                    _consecutiveFailures, _failureThreshold, operationName);
            }
        }
    }

    private void Reset(string operationName)
    {
        lock (_lock)
        {
            _consecutiveFailures = 0;
            _state = CircuitState.Closed;
            _logger.LogInformation("Circuit breaker CLOSED for {OperationName}", operationName);
        }
    }

    public CircuitState State => _state;
}

public enum CircuitState
{
    Closed,
    Open,
    HalfOpen
}

public class CircuitBreakerOpenException : Exception
{
    public CircuitBreakerOpenException(string message) : base(message)
    {
    }
}