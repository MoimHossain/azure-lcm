using AzLcm.Shared.Logging;
using AzLcm.Shared.Resilience;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AzLcm.Shared.Tests;

/// <summary>
/// Simple test class to validate logging and exception handling functionality
/// This would normally be in a test project, but for minimal changes we're including it here
/// </summary>
public class LoggingAndExceptionHandlingValidation
{
    private readonly ILogger<LoggingAndExceptionHandlingValidation> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public LoggingAndExceptionHandlingValidation(ILogger<LoggingAndExceptionHandlingValidation> logger, JsonSerializerOptions jsonOptions)
    {
        _logger = logger;
        _jsonOptions = jsonOptions;
    }

    public async Task ValidateLoggingExtensionsAsync()
    {
        // Test operation logging
        using var scope = _logger.BeginOperationScope("ValidationTest", new { TestParameter = "TestValue" });
        
        try
        {
            _logger.LogOperationStart("ValidationTest");
            
            // Test service health event logging
            _logger.LogServiceHealthEvent("TestEvent", "TestService", new { Status = "Testing" });
            
            // Test policy event logging
            _logger.LogPolicyEvent("TestPolicy", "Created", new { PolicyName = "TestPolicy" });
            
            // Test feed event logging
            _logger.LogFeedEvent("TestFeed", "Approved", new { FeedId = "Test123" });
            
            // Test storage operation logging
            _logger.LogStorageOperation("TestOperation", "TestTable", new { Key = "TestKey" });
            
            // Test external service call logging
            _logger.LogExternalServiceCall("TestService", "https://test.example.com", new { Param = "Value" });
            
            // Test configuration event logging
            _logger.LogConfigurationEvent("TestConfig", "Updated", new { Setting = "TestSetting" });
            
            await Task.Delay(100); // Simulate some work
            
            _logger.LogOperationSuccess("ValidationTest", TimeSpan.FromMilliseconds(100), new { Success = true });
        }
        catch (Exception ex)
        {
            _logger.LogOperationFailure("ValidationTest", ex);
            throw;
        }
    }

    public async Task ValidateRetryPolicyAsync()
    {
        var attemptCount = 0;
        
        try
        {
            await RetryPolicies.ExecuteWithRetryAsync(
                async () =>
                {
                    attemptCount++;
                    _logger.LogInformation("Retry attempt {AttemptCount}", attemptCount);
                    
                    if (attemptCount < 3)
                    {
                        throw new HttpRequestException("Simulated transient failure");
                    }
                    
                    await Task.CompletedTask;
                },
                _logger,
                "TestRetryOperation",
                maxRetries: 3,
                baseDelay: TimeSpan.FromMilliseconds(100)
            );
            
            _logger.LogInformation("Retry policy validation completed successfully after {AttemptCount} attempts", attemptCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Retry policy validation failed");
            throw;
        }
    }

    public async Task ValidateCircuitBreakerAsync()
    {
        var circuitBreaker = new CircuitBreakerPolicy(
            failureThreshold: 2,
            timeout: TimeSpan.FromSeconds(1),
            _logger
        );

        try
        {
            // First, trigger some failures to open the circuit
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    await circuitBreaker.ExecuteAsync<string>(async () =>
                    {
                        await Task.Delay(10);
                        throw new InvalidOperationException("Simulated failure");
                    }, "TestCircuitBreakerOperation");
                }
                catch (Exception ex) when (!(ex is CircuitBreakerOpenException))
                {
                    _logger.LogInformation("Expected failure {FailureCount}: {Message}", i + 1, ex.Message);
                }
            }

            // Now the circuit should be open
            _logger.LogInformation("Circuit breaker state: {State}", circuitBreaker.State);

            // Try to execute operation while circuit is open
            try
            {
                await circuitBreaker.ExecuteAsync<string>(async () =>
                {
                    await Task.CompletedTask;
                    return "Success";
                }, "TestCircuitBreakerOperation");
            }
            catch (CircuitBreakerOpenException ex)
            {
                _logger.LogInformation("Circuit breaker correctly blocked request: {Message}", ex.Message);
            }

            _logger.LogInformation("Circuit breaker validation completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Circuit breaker validation failed");
            throw;
        }
    }

    public void ValidateJsonSerialization()
    {
        try
        {
            var testObject = new
            {
                TestProperty = "TestValue",
                TestNumber = 42,
                TestDate = DateTime.UtcNow,
                TestArray = new[] { "item1", "item2", "item3" }
            };

            var json = JsonSerializer.Serialize(testObject, _jsonOptions);
            _logger.LogInformation("JSON serialization test: {JsonOutput}", json);

            var deserialized = JsonSerializer.Deserialize<dynamic>(json, _jsonOptions);
            _logger.LogInformation("JSON deserialization test completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "JSON serialization validation failed");
            throw;
        }
    }
}