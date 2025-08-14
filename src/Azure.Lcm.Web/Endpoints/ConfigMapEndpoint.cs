

using AzLcm.Shared.Logging;
using AzLcm.Shared.ServiceHealth;
using AzLcm.Shared.Storage;
using Microsoft.AspNetCore.Mvc;

namespace Azure.Lcm.Web.Endpoints;

public class ConfigMapEndpoint
{
    public static async Task<IResult> LoadGeneralConfigAsync(
        [FromServices] ConfigurationStorage configurationStorage,
        [FromServices] ILogger<ConfigMapEndpoint> logger,
        CancellationToken cancellationToken)
    {
        using var scope = logger.BeginOperationScope("LoadGeneralConfig");
        
        try
        {
            logger.LogOperationStart("LoadGeneralConfig");
            var config = await configurationStorage.LoadGeneralConfigAsync(cancellationToken);
            logger.LogOperationSuccess("LoadGeneralConfig", TimeSpan.Zero, new { ConfigLoaded = config != null });
            return Results.Ok(config);
        }
        catch (Exception ex)
        {
            logger.LogOperationFailure("LoadGeneralConfig", ex);
            return Results.Problem(detail: ex.Message, statusCode: 500, title: "Failed to load general configuration");
        }
    }

    public static async Task<IResult> SaveGeneralConfigAsync(
        [FromBody] GeneralConfig config,
        [FromServices] ConfigurationStorage configurationStorage,
        [FromServices] ILogger<ConfigMapEndpoint> logger,
        CancellationToken cancellationToken)
    {
        using var scope = logger.BeginOperationScope("SaveGeneralConfig", config);
        
        try
        {
            logger.LogOperationStart("SaveGeneralConfig", config);
            await configurationStorage.SaveGeneralConfigAsync(config, cancellationToken);
            logger.LogOperationSuccess("SaveGeneralConfig", TimeSpan.Zero, new { ConfigSaved = true });
            return Results.Ok(config);
        }
        catch (Exception ex)
        {
            logger.LogOperationFailure("SaveGeneralConfig", ex, config);
            return Results.Problem(detail: ex.Message, statusCode: 500, title: "Failed to save general configuration");
        }
    }

    public static async Task<IResult> LoadAsync(
        [FromServices] ConfigurationStorage configurationStorage,
        CancellationToken cancellationToken)
    {
        var config = await configurationStorage.LoadConfigAsync(cancellationToken);

        return Results.Ok(config);
    }

    public static async Task<IResult> SaveAsync(
        [FromBody] AreaPathMapConfig config,
        [FromServices] ConfigurationStorage configurationStorage,
        CancellationToken cancellationToken)
    {
        await configurationStorage.SaveConfigAsync(config, cancellationToken);
        return Results.Ok(config);
    }


    public static async Task<IResult> LoadServiceHealthAsync(
        [FromServices] ConfigurationStorage configurationStorage,
        CancellationToken cancellationToken)
    {
        var config = await configurationStorage.LoadServiceHealthConfigAsync(cancellationToken);

        return Results.Ok(config);
    }

    public static async Task<IResult> SaveServiceHealthAsync(
        [FromBody] ServiceHealthConfig config,
        [FromServices] ConfigurationStorage configurationStorage,
        CancellationToken cancellationToken)
    {
        await configurationStorage.SaveServiceHealthConfigAsync(config, cancellationToken);
        return Results.Ok(config);
    }


    public static async Task<IResult> LoadWorkItemTemplatesAsync(
        [FromServices] ConfigurationStorage configurationStorage,
        CancellationToken cancellationToken)
    {
        var config = await configurationStorage.GetWorkItemTemplatesAsync(cancellationToken);

        return Results.Ok(config);
    }

    public static async Task<IResult> SaveWorkItemTemplatesAsync(
        [FromBody] WorkItemTempates config,
        [FromServices] ConfigurationStorage configurationStorage,
        CancellationToken cancellationToken)
    {
        await configurationStorage.SaveWorkItemTemplateAsync(config, cancellationToken);
        return Results.Ok(config);
    }
}