

using AzLcm.Shared;
using AzLcm.Shared.Logging;
using Microsoft.AspNetCore.Mvc;

namespace Azure.Lcm.Web.Endpoints
{
    public class HealthEndpoint
    {
        public static async Task<IResult> Handler(            
            [FromServices] LcmHealthService lcmHealthService, 
            [FromServices] ILogger<HealthEndpoint> logger,
            CancellationToken cancellationToken)
        {
            using var scope = logger.BeginOperationScope("HealthCheck");
            
            try
            {
                logger.LogOperationStart("HealthCheck");
                var healthCheckResponses = await lcmHealthService.CheckAppConfigAsync(cancellationToken);

                logger.LogOperationSuccess("HealthCheck", TimeSpan.Zero, new { HealthyServices = healthCheckResponses.Count });
                return Results.Ok(healthCheckResponses);
            }
            catch (Exception e)
            {
                logger.LogOperationFailure("HealthCheck", e);
                return Results.Problem(detail: e.Message, statusCode: 500, title: "Health check failed");
            }
        }
    }
}
