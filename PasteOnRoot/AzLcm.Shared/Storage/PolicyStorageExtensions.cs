

using AzLcm.Shared.Policy.Models;
using Azure.Data.Tables;
using System.Collections.Concurrent;
using System.ServiceModel.Syndication;

namespace AzLcm.Shared.Storage
{
    public static class PolicyStorageExtensions
    {
        private static string GetPrimaryKey(this PolicyModel item)
        {
            return $"{item.Properties.PolicyType}";
        }

        private static string GetRowKey(this PolicyModel item)
        {
            return $"{item.Name}";
        }

        public static (string partitionKey, string rowKey) GetKeyPair(this PolicyModel item)
        {
            return (GetPrimaryKey(item), GetRowKey(item));
        }

        public static TableEntity ToTableEntity(this PolicyModel policy)
        {
            var (partitionKey, rowKey) = policy.GetKeyPair();

            return new TableEntity(partitionKey, rowKey)
                {
                    { "Id", policy.Id },
                    { "Name", policy.Name},
                    { "PolicyType", policy.Properties.PolicyType },
                    { "DisplayName", $"{policy.Properties.DisplayName}" },
                    { "Description", $"{policy.Properties.Description}" },
                    { "Version", $"{policy.Properties.Version}" },
                    { "Mode", $"{policy.Properties.Mode}" },
                    { "Category", policy.Properties.Metadata.Category },
                    { "Deprecated", policy.Properties.Metadata.Deprecated },
                    { "Preview", policy.Properties.Metadata.Preview },
                    { "MetadataVersion", policy.Properties.Metadata.Version }
                };
        }

        public static PolicyChangeCollection HasChanges(
            this IDictionary<string, object> latestProperties, 
            IDictionary<string, object> oldProperties)
        {
            var delta = new PolicyChangeCollection();
            var keysToCompare = new List<string>() 
            {
                "Id", "Name", "PolicyType", "DisplayName", "Description", "Version","Mode","Category","Deprecated","Preview","MetadataVersion" 
            };
            

            if (latestProperties != null && oldProperties != null)
            {
                foreach (var key in keysToCompare)
                {
                    if (latestProperties[key] != null && oldProperties[key] != null && !latestProperties[key].Equals(oldProperties[key]) )
                    {
                        delta.AddChange(key, latestProperties[key], oldProperties[key]);
                    }
                }
            }
            return delta;
        }
    }
}
