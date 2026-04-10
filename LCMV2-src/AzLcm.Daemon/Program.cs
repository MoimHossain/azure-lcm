using AzLcm.Shared.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddHttpClient<AzureUpdatesFeedService>();
        services.AddHostedService<RssFeedScraperWorker>();
    })
    .ConfigureLogging(logging =>
    {
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Information);
    })
    .Build();

await host.RunAsync();

public class RssFeedScraperWorker : BackgroundService
{
    private readonly AzureUpdatesFeedService _feedService;
    private readonly ILogger<RssFeedScraperWorker> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(6);

    public RssFeedScraperWorker(
        AzureUpdatesFeedService feedService,
        ILogger<RssFeedScraperWorker> logger)
    {
        _feedService = feedService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Azure Lifecycle Sentinel — RSS Feed Scraper started");
        _logger.LogInformation("Polling interval: {Interval}", _interval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Fetching Azure Updates RSS feed at {Time}...", DateTime.UtcNow);

                var updates = await _feedService.FetchUpdatesAsync(stoppingToken);

                _logger.LogInformation("Fetched {Count} updates from Azure Updates RSS feed", updates.Count);

                var grouped = updates
                    .GroupBy(u => u.ChangeType)
                    .OrderByDescending(g => g.Count());

                foreach (var group in grouped)
                {
                    _logger.LogInformation("  {ChangeType}: {Count} entries", group.Key, group.Count());
                }

                var critical = updates.Where(u => u.Severity == AzLcm.Shared.Models.Severity.Critical).ToList();
                if (critical.Count > 0)
                {
                    _logger.LogWarning("⚠️  {Count} CRITICAL severity updates detected:", critical.Count);
                    foreach (var item in critical.Take(10))
                    {
                        _logger.LogWarning("  🔴 {Title} ({Date})", item.Title, item.PublishedDate.ToString("yyyy-MM-dd"));
                    }
                }

                var high = updates.Where(u => u.Severity == AzLcm.Shared.Models.Severity.High).ToList();
                if (high.Count > 0)
                {
                    _logger.LogInformation("🟡 {Count} HIGH severity updates:", high.Count);
                    foreach (var item in high.Take(5))
                    {
                        _logger.LogInformation("  🟠 {Title}", item.Title);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during RSS feed scraping");
            }

            _logger.LogInformation("Next scan at {NextScan}", DateTime.UtcNow.Add(_interval));
            await Task.Delay(_interval, stoppingToken);
        }
    }
}
