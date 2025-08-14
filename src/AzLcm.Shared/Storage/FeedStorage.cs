


using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using System.ServiceModel.Syndication;

namespace AzLcm.Shared.Storage
{
    public class FeedStorage(DaemonConfig daemonConfig, AzureCredentialProvider azureCredentialProvider, ILogger<FeedStorage> logger) : StorageBase
    {
        protected override ILogger Logger => logger;
        protected override AzureCredentialProvider GetAzureCredentialProvider() => azureCredentialProvider;
        protected override string GetStorageAccountName() => daemonConfig.StorageAccountName;

        protected override string GetStorageTableName() => daemonConfig.FeedTableName;

        public async Task<bool> HasSeenAsync(SyndicationItem feed, CancellationToken stoppingToken)
        {
            ArgumentNullException.ThrowIfNull(feed, nameof(feed));

            var (partitionKey, rowKey) = feed.GetKeyPair();

            var existingEntity = await TableClient.GetEntityIfExistsAsync<TableEntity>(partitionKey, rowKey, null, stoppingToken);
            return existingEntity.HasValue;
        }

        public async Task MarkAsSeenAsync(SyndicationItem feed, CancellationToken stoppingToken)
        {
            ArgumentNullException.ThrowIfNull(feed, nameof(feed));

            var (partitionKey, rowKey) = feed.GetKeyPair();

            await TableClient.UpsertEntityAsync(new TableEntity(partitionKey, rowKey)
                {
                    { "FeedId", feed.Id },
                    { "Title", feed.Title.Text }
                }, TableUpdateMode.Merge, stoppingToken);
        }
    }
}
