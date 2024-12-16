

using AzLcm.Shared.ServiceHealth;
using AzLcm.Shared.Storage;
using Microsoft.AspNetCore.Mvc;

namespace Azure.Lcm.Web.Endpoints;

public class ConfigMapEndpoint
{
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