

using AzLcm.Daemon;
using AzLcm.Shared;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddLogging(logging =>
{
    logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
    logging.AddConsole();
    logging.AddDebug();
});
builder.Services.AddSingleton(services =>
{
    var jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true
    };
    jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    return jsonSerializerOptions;
});

builder.Services.AddHttpClient();
builder.Services.AddRequiredServices();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
