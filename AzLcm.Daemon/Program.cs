

using AzLcm.Daemon;
using AzLcm.Shared;
using AzLcm.Shared.Storage;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddSingleton<DaemonConfig>();
builder.Services.AddSingleton<FeedStorage>();
builder.Services.AddSingleton<AzUpdateSyndicationFeed>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
