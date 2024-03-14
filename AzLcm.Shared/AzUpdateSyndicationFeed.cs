

using System.ServiceModel.Syndication;
using System.Xml;

namespace AzLcm.Shared
{
    public class AzUpdateSyndicationFeed(HttpClient httpClient)
    {
        public async Task<IEnumerable<SyndicationItem>> ReadAsync()
        {
            var url = "https://azurecomcdn.azureedge.net/en-us/updates/feed/";

            var configuredUri = Environment.GetEnvironmentVariable("AZURE_UPDATE_FEED_URI");
            if (!string.IsNullOrEmpty(configuredUri))
            {
                url = configuredUri;
            }

            var feedContent = await httpClient.GetStringAsync (url);
            using var reader = XmlReader.Create(new StringReader(feedContent));
            var feed = SyndicationFeed.Load(reader);

            return feed.Items;
        }
    }
}
