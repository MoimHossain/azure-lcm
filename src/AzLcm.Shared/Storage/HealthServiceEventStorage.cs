


using AzLcm.Shared.ServiceHealth;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;

namespace AzLcm.Shared.Storage
{
    public class HealthServiceEventStorage(DaemonConfig daemonConfig, AzureCredentialProvider azureCredentialProvider, ILogger<HealthServiceEventStorage> logger) : StorageBase
    {
        protected override ILogger Logger => logger;
        protected override AzureCredentialProvider GetAzureCredentialProvider() => azureCredentialProvider;
        protected override string GetStorageAccountName() => daemonConfig.StorageAccountName;

        protected override string GetStorageTableName() => daemonConfig.ServiceHealthTableName;


        public async Task<bool> HasSeenAsync(ServiceHealthEvent svcHealthEvent, CancellationToken stoppingToken)
        {
            ArgumentNullException.ThrowIfNull(svcHealthEvent, nameof(svcHealthEvent));

            var (partitionKey, rowKey) = svcHealthEvent.GetKeyPair();

            var existingEntity = await TableClient.GetEntityIfExistsAsync<TableEntity>(partitionKey, rowKey, null, stoppingToken);
            return existingEntity.HasValue;
        }

        public async Task MarkAsSeenAsync(ServiceHealthEvent svcHealthEvent, CancellationToken stoppingToken)
        {
            ArgumentNullException.ThrowIfNull(svcHealthEvent, nameof(svcHealthEvent));

            var (partitionKey, rowKey) = svcHealthEvent.GetKeyPair();

            await TableClient.UpsertEntityAsync(new TableEntity(partitionKey, rowKey)
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
