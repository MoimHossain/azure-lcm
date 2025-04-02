

using AzLcm.Shared.Policy.Models;
using Azure.Data.Tables;
using System.Collections.Immutable;

namespace AzLcm.Shared.Storage
{
    public class PolicyStorage(DaemonConfig daemonConfig, AzureCredentialProvider azureCredentialProvider) : StorageBase
    {
        protected override AzureCredentialProvider GetAzureCredentialProvider() => azureCredentialProvider;
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

        internal class GitHubSHAMilestone
        {
            internal const string PartitionKey = "AZ-LCM";
            internal const string RowKey = "GITHUB-AZPOLICY-LastCommitSHA";

        }

        public async Task<string> GetLastProcessedShaAsync(CancellationToken cancellationToken)
        {   
            var existingEntity = await TableClient
                .GetEntityIfExistsAsync<TableEntity>(GitHubSHAMilestone.PartitionKey, GitHubSHAMilestone.RowKey, null, cancellationToken);

            if (existingEntity.HasValue && existingEntity.Value != null)
            {
                var properties = existingEntity.Value.ToImmutableDictionary();
                if (properties["SHA"] != null)
                {
                    return $"{properties["SHA"]}";
                }
            }
            return "fd3caeca0adb57063a394904fe6f11a0759a1f18";
        }

        public async Task UpdateLastProcessedShaAsync(string sha, CancellationToken stoppingToken)
        {
            var entity = new TableEntity(GitHubSHAMilestone.PartitionKey, GitHubSHAMilestone.RowKey)
            {
                { "SHA", sha }
            };
            await TableClient.UpsertEntityAsync(entity, TableUpdateMode.Merge, stoppingToken);
        }
    }
}
