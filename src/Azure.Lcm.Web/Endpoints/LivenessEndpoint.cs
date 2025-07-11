

using AzLcm.Shared.Logging;
using Microsoft.AspNetCore.Mvc;

namespace Azure.Lcm.Web.Endpoints
{
    public class LivenessEndpoint
    {
        public static async Task<IResult> Handler(
            [FromServices] ILogger<LivenessEndpoint> logger,
            CancellationToken cancellationToken)
        {
            using var scope = logger.BeginOperationScope("LivenessCheck");
            
            try
            {
                logger.LogOperationStart("LivenessCheck");
                await Task.CompletedTask;
                logger.LogOperationSuccess("LivenessCheck", TimeSpan.Zero, new { Status = "Alive" });
                return Results.Ok(new { Ok = true, Status = "Alive", Timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                logger.LogOperationFailure("LivenessCheck", ex);
                return Results.Problem(detail: ex.Message, statusCode: 500, title: "Liveness check failed");
            }
        }
    }
}
