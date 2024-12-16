

using Azure.Identity;
using AzLcm.Shared;
using Azure.Lcm.Web.Endpoints;
using System.Text.Json.Serialization;
using System.Text.Json;
using Azure.Lcm.Web;
using AzLcm.Shared.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();
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
builder.Services.AddHostedService<LcmBackgroundService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.UseCors();

var apiGroup = app.MapGroup("api");
apiGroup.MapGet("/health", HealthEndpoint.Handler).WithName("Health API").WithDisplayName("Service Health API").WithOpenApi();
apiGroup.MapGet("/live", LivenessEndpoint.Handler).WithName("Liveness API").WithDisplayName("Liveness API").WithOpenApi();
apiGroup.MapGet("/traces", TraceEndpoint.Handler).WithName("Tracing API").WithDisplayName("Tracing API").WithOpenApi();

app.Run();
