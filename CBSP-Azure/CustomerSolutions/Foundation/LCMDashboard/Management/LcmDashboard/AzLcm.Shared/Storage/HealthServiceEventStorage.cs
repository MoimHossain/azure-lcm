


using AzLcm.Shared.ServiceHealth;
using Azure.Data.Tables;
using Azure.Identity;

namespace AzLcm.Shared.Storage
{
    public class HealthServiceEventStorage(DaemonConfig daemonConfig)
    {
        private readonly TableClient tableClient = new(new Uri($"https://{daemonConfig.StorageAccountName}.table.core.windows.net"), daemonConfig.ServiceHealthTableName, new DefaultAzureCredential());

        public async Task EnsureTableExistsAsync()
        {
            await tableClient.CreateIfNotExistsAsync();
        }

        public async Task<bool> HasSeenAsync(ServiceHealthEvent svcHealthEvent, CancellationToken stoppingToken)
        {
            ArgumentNullException.ThrowIfNull(svcHealthEvent, nameof(svcHealthEvent));

            var (partitionKey, rowKey) = svcHealthEvent.GetKeyPair();

            var existingEntity = await tableClient.GetEntityIfExistsAsync<TableEntity>(partitionKey, rowKey, null, stoppingToken);
            return existingEntity.HasValue;
        }

        public async Task MarkAsSeenAsync(ServiceHealthEvent svcHealthEvent, CancellationToken stoppingToken)
        {
            ArgumentNullException.ThrowIfNull(svcHealthEvent, nameof(svcHealthEvent));

            var (partitionKey, rowKey) = svcHealthEvent.GetKeyPair();

            await tableClient.UpsertEntityAsync(new TableEntity(partitionKey, rowKey)
                {
                    { "Name", svcHealthEvent.Name },
                    { "Service", svcHealthEvent.Service },
                    { "LastUpdate", svcHealthEvent.LastUpdate },
                    { "URI", svcHealthEvent.Url },
                    { "Title", svcHealthEvent.Title }
                }, TableUpdateMode.Merge, stoppingToken);
        }
    }
}
