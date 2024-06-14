

using AzLcm.Shared.AzureUpdates.Model;
using System.ServiceModel.Syndication;

namespace AzLcm.Shared.Storage
{
    public static class FeedStorageExtensions
    {
        private static string GetPrimaryKey(this SyndicationItem item)
        {   
            return $"{item.PublishDate.Year}{item.PublishDate.Month}";
        }

        private static string GetRowKey(this SyndicationItem item)
        {
            return item.Id;
        }

        public static (string partitionKey, string rowKey) GetKeyPair(this SyndicationItem item)
        {
            return (GetPrimaryKey(item), GetRowKey(item));
        }

        
        private static string? GetPrimaryKey(this AzFeedItem item)
        {
            return $"{item.PublishedDate.Year}{item.PublishedDate.Month}";
        }

        private static string? GetRowKey(this AzFeedItem item)
        {
            return item.GetID();
        }

        public static (string? partitionKey, string? rowKey) GetKeyPair(this AzFeedItem item)
        {
            return (GetPrimaryKey(item), GetRowKey(item));
        }
    }
}
