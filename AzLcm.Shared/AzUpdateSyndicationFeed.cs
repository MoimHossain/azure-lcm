

using System.ServiceModel.Syndication;
using System.Xml;

namespace AzLcm.Shared
{
    public class AzUpdateSyndicationFeed(DaemonConfig daemonConfig, HttpClient httpClient)
    {
        public async Task<IEnumerable<SyndicationItem>> ReadAsync()
        {
            var url = "https://azurecomcdn.azureedge.net/en-us/updates/feed/";

            if (!string.IsNullOrWhiteSpace(daemonConfig.AzureUpdateFeedUri))
            {
                url = daemonConfig.AzureUpdateFeedUri;
            }

            var feedContent = await httpClient.GetStringAsync (url);
            using var reader = XmlReader.Create(new StringReader(feedContent));
            var feed = SyndicationFeed.Load(reader);

            return feed.Items;
        }
    }
}
