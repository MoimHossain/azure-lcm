

using AzLcm.Shared.Policy.Models;
using Azure.Data.Tables;
using System.Collections.Immutable;
using System.ServiceModel.Syndication;

namespace AzLcm.Shared.Storage
{
    public class PolicyStorage(DaemonConfig daemonConfig)
    {
        private readonly TableClient tableClient = new(daemonConfig.StorageConnectionString, daemonConfig.PolicyTableName);

        public async Task EnsureTableExistsAsync()
        {
            await tableClient.CreateIfNotExistsAsync();
        }

        public async Task<PolicyModelChange> HasSeenAsync(PolicyModel policy)
        {
            ArgumentNullException.ThrowIfNull(policy, nameof(policy));

            var latestChanges = policy.ToTableEntity().ToImmutableDictionary();

            var (partitionKey, rowKey) = policy.GetKeyPair();
            var existingEntity = await tableClient.GetEntityIfExistsAsync<TableEntity>(partitionKey, rowKey);

            if (!existingEntity.HasValue)
            {
                return new PolicyModelChange(ChangeKind.Add, policy, null);
            }

            if (existingEntity.Value != null && latestChanges != null)
            {   
                var oldProperties = existingEntity.Value.ToImmutableDictionary();

                var delta = latestChanges.HasChanges(oldProperties);

                if (delta.HasChanges)
                {
                    return new PolicyModelChange(ChangeKind.Update, policy, delta);
                }
            }

            return new PolicyModelChange(ChangeKind.None, policy, null);
        }

        public async Task MarkAsSeenAsync(PolicyModel policy, CancellationToken stoppingToken)
        {
            ArgumentNullException.ThrowIfNull(policy, nameof(policy));

            await tableClient.UpsertEntityAsync(policy.ToTableEntity(), TableUpdateMode.Merge, stoppingToken);
        }
    }
}
