

using Azure.Data.Tables;
using System.ServiceModel.Syndication;

namespace AzLcm.Shared.Storage
{
    public class FeedStorage(DaemonConfig daemonConfig)
    {
        private readonly TableClient tableClient = new(daemonConfig.StorageConnectionString, daemonConfig.FeedTableName);

        public async Task EnsureTableExistsAsync()
        {
            await tableClient.CreateIfNotExistsAsync();
        }

        public async Task<bool> HasSeenAsync(SyndicationItem feed, CancellationToken stoppingToken)
        {
            ArgumentNullException.ThrowIfNull(feed, nameof(feed));

            var (partitionKey, rowKey) = feed.GetKeyPair();

            var existingEntity = await tableClient.GetEntityIfExistsAsync<TableEntity>(partitionKey, rowKey, null, stoppingToken);
            return existingEntity.HasValue;
        }

        public async Task MarkAsSeenAsync(SyndicationItem feed, CancellationToken stoppingToken)
        {
            ArgumentNullException.ThrowIfNull(feed, nameof(feed));

            var (partitionKey, rowKey) = feed.GetKeyPair();

            await tableClient.UpsertEntityAsync(new TableEntity(partitionKey, rowKey)
                {
                    { "FeedId", feed.Id },
                    { "Title", feed.Title.Text }
                }, TableUpdateMode.Merge, stoppingToken);
        }
    }
}
