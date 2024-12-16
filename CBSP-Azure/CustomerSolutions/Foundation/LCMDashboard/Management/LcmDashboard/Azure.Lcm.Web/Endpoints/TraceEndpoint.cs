

using AzLcm.Shared.Logging;
using Microsoft.AspNetCore.Mvc;

namespace Azure.Lcm.Web.Endpoints;

public class TraceEndpoint
{
    public static async Task<IResult> Handler([FromServices] WebLogInMemoryStorage webLogInMemoryStorage)  
    {
        await Task.CompletedTask;

        return Results.Ok(webLogInMemoryStorage.GetLogEntries());
    }
}
