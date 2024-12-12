

using AzLcm.Shared.Policy.Models;
using Azure.Data.Tables;
using System.Collections.Immutable;

namespace AzLcm.Shared.Storage
{
    public class PolicyStorage(DaemonConfig daemonConfig) : StorageBase
    {
        protected override string GetStorageAccountName() => daemonConfig.StorageAccountName;

        protected override string GetStorageTableName() => daemonConfig.PolicyTableName;

        public async Task<PolicyModelChange> HasSeenAsync(PolicyModel policy)
        {
            ArgumentNullException.ThrowIfNull(policy, nameof(policy));

            var latestChanges = policy.ToTableEntity().ToImmutableDictionary();

            var (partitionKey, rowKey) = policy.GetKeyPair();
            var existingEntity = await TableClient.GetEntityIfExistsAsync<TableEntity>(partitionKey, rowKey);

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

            await TableClient.UpsertEntityAsync(policy.ToTableEntity(), TableUpdateMode.Merge, stoppingToken);
        }
    }
}
