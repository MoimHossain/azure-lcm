

using AzLcm.Shared;
using AzLcm.Shared.Logging;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;
using System.Text.Json;
using Azure.Lcm.Daemon;



var builder = Host.CreateApplicationBuilder(args);


builder.Services.Configure<Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration>(config =>
{
    config.SetAzureTokenCredential(new DefaultAzureCredential());
});

//builder.Services.AddLogging(logging =>
//{
//    logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
//    logging.AddConsole();
//    logging.AddApplicationInsights();
//    logging.AddDebug();
//});

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddSingleton<WebLogInMemoryStorage>();
builder.Logging.AddInMemoryLogger(configuration =>
{
    configuration.LogLevelToColorMap[LogLevel.Warning] = ConsoleColor.DarkCyan;
    configuration.LogLevelToColorMap[LogLevel.Error] = ConsoleColor.DarkRed;
});

if (!string.IsNullOrWhiteSpace(DaemonConfig.AppInsightConnectionString))
{
    builder.Logging.AddApplicationInsights(
        configureTelemetryConfiguration: (config) =>
            config.ConnectionString = DaemonConfig.AppInsightConnectionString,
            configureApplicationInsightsLoggerOptions: (options) => { }
    );
}
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