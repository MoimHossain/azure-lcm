

using AzLcm.Shared;
using AzLcm.Shared.AzureDevOps;
using AzLcm.Shared.Cognition;
using AzLcm.Shared.PageScrapping;
using AzLcm.Shared.Policy;
using AzLcm.Shared.Storage;

namespace AzLcm.Daemon
{
    public class Worker(
        DaemonConfig config,
        FeedStorage feedStorage,
        PolicyStorage policyStorage,
        DevOpsClient devOpsClient,
        CognitiveService cognitiveService,
        HtmlExtractor htmlExtractor,
        PolicyReader policyReader,
        AzUpdateSyndicationFeed azUpdateSyndicationFeed,
        WorkItemTemplateStorage workItemTemplateStorage,
        ILogger<Worker> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var azdoConfig = config.GetAzureDevOpsClientConfig();
            var connectionData = await devOpsClient.GetConnectionDataAsync(azdoConfig.orgName);

            if (connectionData.AuthenticatedUser == null ||
                string.IsNullOrWhiteSpace(connectionData.AuthenticatedUser.SubjectDescriptor))
            {
                logger.LogError("Failed to connect to Azure DevOps.");
                return;
            }

            await feedStorage.EnsureTableExistsAsync();
            await policyStorage.EnsureTableExistsAsync();
            

            while (!stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                await ProcessPolicyAsync(stoppingToken);
                await ProcessFeedAsync(stoppingToken);
                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task ProcessPolicyAsync(CancellationToken stoppingToken)
        {
            if (config.ProcessPolicy)
            {
                await policyReader.ReadPoliciesAsync(async policy =>
                {
                    var difference = await policyStorage.HasSeenAsync(policy);
                    if (difference.ChangeKind != Shared.Policy.Models.ChangeKind.None)
                    {
                        // New or updated policy

                        await policyStorage.MarkAsSeenAsync(policy, stoppingToken);
                    }
                }, stoppingToken);
            }
        }

        private async Task ProcessFeedAsync(CancellationToken stoppingToken)
        {   
            if (config.ProcessFeed)
            {
                var azDevOpsConfig = config.GetAzureDevOpsClientConfig();
                var template = await workItemTemplateStorage.GetWorkItemTemplateAsync(stoppingToken);
                var feeds = await azUpdateSyndicationFeed.ReadAsync(stoppingToken);
                var processedCount = 0;
                foreach (var feed in feeds)
                {
                    var seen = await feedStorage.HasSeenAsync(feed, stoppingToken);

                    if (!seen)
                    {
                        ++processedCount;

                        var fragments = await htmlExtractor.GetHtmlExtractedFragmentsAsync(feed);

                        var verdict = await cognitiveService.AnalyzeAsync(feed, fragments, stoppingToken);
                        await devOpsClient.CreateWorkItemAsync(azDevOpsConfig.orgName, template, feed, verdict);

                        await feedStorage.MarkAsSeenAsync(feed, stoppingToken);
                    }
                }

                logger.LogInformation("Process {count} items, out of {total} feeds.", processedCount, feeds.Count());
            }
        }
    }
}
