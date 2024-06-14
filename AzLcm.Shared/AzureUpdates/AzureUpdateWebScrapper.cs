



using AngleSharp;
using AngleSharp.Html.Parser;
using AzLcm.Shared.AzureUpdates.Model;
using AzLcm.Shared.PageScrapping;
using AzLcm.Shared.Policy.Models;
using System.Globalization;

namespace AzLcm.Shared.AzureUpdates
{
    public class AzureUpdateWebScrapper(IHttpClientFactory httpClientFactory)
    {
        public async Task ReadAsync(Func<AzFeedItem, Task> work, CancellationToken stoppingToken)
        {
            var azureUpdateUri = "https://azure.microsoft.com";
            var enUSCulture = new CultureInfo("en-US");

            using var httpClient = httpClientFactory.CreateClient();
            var htmlContent = await httpClient.GetStringAsync($"{azureUpdateUri}/{enUSCulture}/updates/", stoppingToken);

            var parser = BrowsingContext.New(Configuration.Default).GetService<IHtmlParser>();
            if (parser != null)
            {
                var document = parser.ParseDocument(htmlContent);

                foreach (var updateRowDiv in document.QuerySelectorAll("div.update-row"))
                {
                    var updateUrl = updateRowDiv.QuerySelector("a").SafeReadHref();
                    if (!string.IsNullOrWhiteSpace(updateUrl))
                    {
                        var feedItem = await ReadUpdateDetailsPageAsync(
                            azureUpdateUri, enUSCulture, parser, updateUrl, stoppingToken);

                        if(feedItem != null && feedItem.Validate())
                        {
                            await work(feedItem);
                        }
                    }
                }
            }
        }

        private async Task<AzFeedItem?> ReadUpdateDetailsPageAsync(
            string azureUpdateUri, CultureInfo enUSCulture, 
            IHtmlParser? parser, string updateUrl, CancellationToken stoppingToken)
        {
            if(parser != null && !string.IsNullOrWhiteSpace(updateUrl))
            {
                var feedHttpClient = httpClientFactory.CreateClient();
                var feedHtmlContent = await feedHttpClient.GetStringAsync($"{azureUpdateUri}{updateUrl}", stoppingToken);
                var feedDocument = parser.ParseDocument(feedHtmlContent);

                // The following body reading is mutating the DOM - so ORDER of reading is important
                var bodyElement = feedDocument.QuerySelector("div.column.small-12");
                if (bodyElement != null)
                {
                    var title = feedDocument.QuerySelector("h1").SafeReadInnerText();
                    var publishedDate = feedDocument.QuerySelector("h6").SafeReadDate(enUSCulture);
                    var tags = feedDocument.QuerySelectorAll("ul.tags li").Select(x => x.TextContent).ToArray();

                    if (bodyElement.Children != null && bodyElement.Children.Length > 1)
                    {
                        bodyElement.RemoveChild(bodyElement.Children[bodyElement.Children.Length - 1]);
                    }

                    var htmlBody = bodyElement.SafeReadInnerHtml();
                    var updateBody = bodyElement.SafeReadInnerText();

                    return new AzFeedItem(title, publishedDate, tags, htmlBody, updateBody, updateUrl);
                }
            }
            return default;
        }
    }
}
