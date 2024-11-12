using System.ServiceModel.Syndication;

namespace AzLcm.Shared.Storage
{
    public static class FeedStorageExtensions
    {
        private readonly static char[] SEPCHARS = ['/'];
        private static string GetPrimaryKey(this SyndicationItem item)
        {   
            return $"{item.PublishDate.Year}{item.PublishDate.Month}";
        }

        private static string GetRowKey(this SyndicationItem item)
        {
            if (!string.IsNullOrWhiteSpace(item.Id))
            {   
                if (Uri.TryCreate(item.Id, UriKind.Absolute, out _))
                {
                    var segments = item.Id.Split(SEPCHARS, StringSplitOptions.RemoveEmptyEntries);
                    if (segments != null && segments.Length > 0)
                    {
                        return segments[^1];
                    }
                }                
            }
            return item.Id;
        }

        public static (string partitionKey, string rowKey) GetKeyPair(this SyndicationItem item)
        {
            return (GetPrimaryKey(item), GetRowKey(item));
        }

    }
}
