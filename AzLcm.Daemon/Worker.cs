

using AzLcm.Shared;
using AzLcm.Shared.AzureDevOps;
using AzLcm.Shared.AzureUpdates;
using AzLcm.Shared.Cognition;
using AzLcm.Shared.PageScrapping;
using AzLcm.Shared.Policy;
using AzLcm.Shared.ServiceHealth;
using AzLcm.Shared.Storage;

namespace AzLcm.Daemon
{
    public class Worker(
        DaemonConfig config,
        FeedStorage feedStorage,
        PolicyStorage policyStorage,
        HealthServiceEventStorage healthServiceEventStorage,
        DevOpsClient devOpsClient,
        CognitiveService cognitiveService,
        HtmlExtractor htmlExtractor,
        PolicyReader policyReader,
        ServiceHealthReader serviceHealthReader,
        AzUpdateSyndicationFeed azUpdateSyndicationFeed,
        AzureUpdateWebScrapper azureUpdateWebScrapper,
        PromptTemplateStorage promptTemplateStorage,
        WorkItemTemplateStorage workItemTemplateStorage,
        ILogger<Worker> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var azdoConfig = config.GetAzureDevOpsClientConfig();
            var connectionData = await devOpsClient.GetConnectionDataAsync(azdoConfig.orgName, stoppingToken);

            if (connectionData.AuthenticatedUser == null ||
                string.IsNullOrWhiteSpace(connectionData.AuthenticatedUser.SubjectDescriptor))
            {
                logger.LogError("Failed to connect to Azure DevOps.");
                return;
            }

            await feedStorage.EnsureTableExistsAsync();
            await policyStorage.EnsureTableExistsAsync();
            await healthServiceEventStorage.EnsureTableExistsAsync();
                        
            while (!stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                await ProcessServiceHealthAsync(stoppingToken);
                await ProcessPolicyAsync(stoppingToken);
                await ProcessFeedAsync(stoppingToken);                
                
                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task ProcessServiceHealthAsync(CancellationToken stoppingToken)
        {
            if (config.ProcessServiceHealth)
            {
                var azDevOpsConfig = config.GetAzureDevOpsClientConfig();
                var template = await workItemTemplateStorage.GetServiceHealthWorkItemTemplateAsync(stoppingToken);

                var processedCount = 0;
                var totalEventCount = 0;

                await serviceHealthReader.ProcessAsync(async serviceHealthEvent => 
                {
                    ++totalEventCount;
                    var seen = await healthServiceEventStorage.HasSeenAsync(serviceHealthEvent, stoppingToken);

                    if (!seen)
                    {
                        ++processedCount;

                        await devOpsClient.CreateWorkItemFromServiceHealthAsync(
                            azDevOpsConfig.orgName, template, serviceHealthEvent, stoppingToken);

                        await healthServiceEventStorage.MarkAsSeenAsync(serviceHealthEvent, stoppingToken);
                    }
                }, stoppingToken);
                logger.LogInformation("Process {processedCount} items, out of {totalEventCount} service health events.", 
                    processedCount, totalEventCount);
            }
        }

        private async Task ProcessPolicyAsync(CancellationToken stoppingToken)
        {
            if (config.ProcessPolicy)
            {
                var azDevOpsConfig = config.GetAzureDevOpsClientConfig();
                var template = await workItemTemplateStorage.GetPolicyWorkItemTemplateAsync(stoppingToken);

                var processedCount = 0;
                var totalPolicyCount = 0;
                await policyReader.ReadPoliciesAsync(async policy =>
                {
                    ++totalPolicyCount;
                    
                    var difference = await policyStorage.HasSeenAsync(policy);
                    logger.LogInformation("Evaluated Policy changes, id={policyId}, change kind={changeKind}", policy.Id, difference.ChangeKind);

                    if (difference.ChangeKind != Shared.Policy.Models.ChangeKind.None)
                    {
                        ++processedCount;
                        await devOpsClient.CreateWorkItemFromPolicyAsync(
                            azDevOpsConfig.orgName, template, difference, stoppingToken);
                        await policyStorage.MarkAsSeenAsync(policy, stoppingToken);
                    }
                }, stoppingToken);
                logger.LogInformation("Process {processedPolicyCount} items, out of {totalPolicyCount} feeds.", processedCount, totalPolicyCount);
            }
        }

        private async Task ProcessFeedAsync(CancellationToken stoppingToken)
        {   
            if (config.ProcessFeed)
            {
                var azDevOpsConfig = config.GetAzureDevOpsClientConfig();
                var template = await workItemTemplateStorage.GetFeedWorkItemTemplateAsync(stoppingToken);
                var promptTemplate = await promptTemplateStorage.GetFeedPromptAsync(stoppingToken);


                var processedCount = 0;
                await azureUpdateWebScrapper.ReadAsync(async (feedItem) => 
                {
                    var seen = await feedStorage.HasSeenAsync(feedItem, stoppingToken);

                    if (!seen)
                    {
                        ++processedCount;

                        var verdict = await cognitiveService.AnalyzeV2Async(feedItem, promptTemplate, stoppingToken);
                        await devOpsClient.CreateWorkItemFromFeedAsync(
                            azDevOpsConfig.orgName, template, feedItem, verdict, stoppingToken);

                        await feedStorage.MarkAsSeenAsync(feedItem, stoppingToken);
                    }
                }, stoppingToken);
                logger.LogInformation("Processed {count} items", processedCount);

                /*
                var feeds = await azUpdateSyndicationFeed.ReadAsync(stoppingToken);
                var processedCount = 0;
                foreach (var feed in feeds)
                {
                    var seen = await feedStorage.HasSeenAsync(feed, stoppingToken);

                    if (!seen)
                    {
                        ++processedCount;

                        var fragments = await htmlExtractor.GetHtmlExtractedFragmentsAsync(feed);

                        var verdict = await cognitiveService.AnalyzeAsync(feed, fragments, promptTemplate, stoppingToken);
                        await devOpsClient.CreateWorkItemFromFeedAsync(azDevOpsConfig.orgName, template, feed, verdict);

                        await feedStorage.MarkAsSeenAsync(feed, stoppingToken);
                    }
                }

                logger.LogInformation("Process {processedFeedCount} items, out of {totalFeedCount} feeds.", processedCount, feeds.Count());
                */
            }
        }
    }
}
