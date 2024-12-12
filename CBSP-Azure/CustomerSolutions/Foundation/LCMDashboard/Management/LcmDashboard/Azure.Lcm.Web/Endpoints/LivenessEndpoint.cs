

using Microsoft.AspNetCore.Mvc;

namespace Azure.Lcm.Web.Endpoints
{
    public class LivenessEndpoint
    {
        public static async Task<IResult> Handler(
            [FromServices] ILogger<HealthEndpoint> logger,
            CancellationToken cancellationToken)
        {
            logger.LogInformation("Liveness endpoint invoked");
            await Task.CompletedTask;

            return Results.Ok(new { Ok = true });
        }
    }
}
