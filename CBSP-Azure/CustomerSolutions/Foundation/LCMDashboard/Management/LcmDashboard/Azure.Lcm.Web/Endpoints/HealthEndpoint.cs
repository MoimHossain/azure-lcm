

using AzLcm.Shared;
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
            try
            {
                logger.LogInformation("Health endpoint invoked");
                var healthCheckResponses = await lcmHealthService.CheckAppConfigAsync(cancellationToken);

                return Results.Ok(healthCheckResponses);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error processing request");
                return Results.Problem(e.Message);
            }
        }
    }
}
