

using AzLcm.Shared;
using AzLcm.Shared.Cognition;
using AzLcm.Shared.Storage;

namespace AzLcm.Daemon
{
    public class Worker(
        AzUpdateSyndicationFeed azUpdateSyndicationFeed,
        CognitiveService cognitiveService,
        FeedStorage feedStorage,
        ILogger<Worker> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await feedStorage.EnsureTableExistsAsync();

            while (!stoppingToken.IsCancellationRequested)
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }

                var feeds = await azUpdateSyndicationFeed.ReadAsync();
                var processedCount = 0;
                foreach (var feed in feeds)
                {
                    var seen = await feedStorage.HasSeenAsync(feed);
                    if(!seen)
                    {
                        ++processedCount;

                        await cognitiveService.AnalyzeAsync(feed);

                        await feedStorage.MarkAsSeenAsync(feed);
                    }
                }

                logger.Log(LogLevel.Information, $"Process {processedCount} items, out of {feeds.Count()} feeds.");
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
