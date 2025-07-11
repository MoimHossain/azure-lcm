

using AzLcm.Shared.Logging;
using Microsoft.AspNetCore.Mvc;

namespace Azure.Lcm.Web.Endpoints;

public class TraceEndpoint
{
    public static async Task<IResult> Handler(
        [FromServices] WebLogInMemoryStorage webLogInMemoryStorage,
        [FromServices] ILogger<TraceEndpoint> logger)  
    {
        using var scope = logger.BeginOperationScope("GetTraces");
        
        try
        {
            logger.LogOperationStart("GetTraces");
            await Task.CompletedTask;
            
            var logEntries = webLogInMemoryStorage.GetLogEntries();
            logger.LogOperationSuccess("GetTraces", TimeSpan.Zero, new { LogEntryCount = logEntries.Count() });
            
            return Results.Ok(logEntries);
        }
        catch (Exception ex)
        {
            logger.LogOperationFailure("GetTraces", ex);
            return Results.Problem(detail: ex.Message, statusCode: 500, title: "Failed to retrieve traces");
        }
    }
}
