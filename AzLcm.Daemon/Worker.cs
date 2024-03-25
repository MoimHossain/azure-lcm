

using AzLcm.Shared;
using AzLcm.Shared.AzureDevOps;
using AzLcm.Shared.Cognition;
using AzLcm.Shared.PageScrapping;
using AzLcm.Shared.Storage;
using System.ServiceModel.Syndication;

namespace AzLcm.Daemon
{
    public class Worker(
        DaemonConfig config,
        FeedStorage feedStorage,
        DevOpsClient devOpsClient,        
        CognitiveService cognitiveService,
        HtmlExtractor htmlExtractor,
        AzUpdateSyndicationFeed azUpdateSyndicationFeed,
        WorkItemTemplateStorage workItemTemplateStorage,
        ILogger<Worker> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var azdoConfig = config.GetAzureDevOpsClientConfig();
            var connectionData = await devOpsClient.GetConnectionDataAsync(azdoConfig.orgName);

            if(connectionData.AuthenticatedUser == null || 
                string.IsNullOrWhiteSpace(connectionData.AuthenticatedUser.SubjectDescriptor))
            {
                logger.LogError("Failed to connect to Azure DevOps.");
                return;
            }

            await feedStorage.EnsureTableExistsAsync();
            var template = await workItemTemplateStorage.GetWorkItemTemplateAsync();

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

                    if (!seen)
                    {
                        ++processedCount;

                        var fragments = await GetHtmlExtractedFragmentsAsync(feed);

                        var verdict = await cognitiveService.AnalyzeAsync(feed, fragments);
                        await devOpsClient.CreateWorkItemAsync(azdoConfig.orgName, template, feed, verdict);

                        await feedStorage.MarkAsSeenAsync(feed);
                    }
                }

                logger.Log(LogLevel.Information, $"Process {processedCount} items, out of {feeds.Count()} feeds.");
                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task<List<HtmlFragment>> GetHtmlExtractedFragmentsAsync(SyndicationItem feed)
        {
            if (feed.Links != null && feed.Links.Count != 0)
            {
                var content = await htmlExtractor.ExtractAsync(feed.Links[0].Uri);

                return content;
            }
            return new List<HtmlFragment>();
        }
    }
}
