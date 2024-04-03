

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
                return new PolicyModelChange(ChangeKind.Add, policy, latestChanges);
            }

            if (existingEntity.Value != null && latestChanges != null)
            {
                var differences = new Dictionary<string, object>();
                var oldProperties = existingEntity.Value.ToImmutableDictionary();
                // TODO : Optimize this following later
                foreach (var oldKey in oldProperties.Keys)
                {
                    if (!latestChanges.ContainsKey(oldKey))
                    {
                        differences[oldKey] = oldProperties[oldKey];
                    }
                    else if (latestChanges[oldKey] != oldProperties[oldKey])
                    {
                        differences[oldKey] = oldProperties[oldKey];
                    }
                }
                
                foreach (var newKey in latestChanges.Keys)
                {
                    if (!oldProperties.ContainsKey(newKey))
                    {
                        differences[newKey] = latestChanges[newKey];
                    }
                    else if (latestChanges[newKey] != oldProperties[newKey])
                    {
                        differences[newKey] = oldProperties[newKey];
                    }
                }

                if (differences.Count > 0)
                {
                    return new PolicyModelChange(ChangeKind.Update, policy, differences);
                }
            }

            return new PolicyModelChange(ChangeKind.None, policy, null);
        }

        public async Task MarkAsSeenAsync(PolicyModel policy)
        {
            ArgumentNullException.ThrowIfNull(policy, nameof(policy));

            await tableClient.UpsertEntityAsync(policy.ToTableEntity());
        }
    }
}
