

using Abot2.Crawler;
using Abot2.Poco;
using AngleSharp.Html.Dom;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using System.ServiceModel.Syndication;


namespace AzLcm.Shared.PageScrapping
{
    public class HtmlExtractor(ILogger<HtmlExtractor> logger)
    {
        public async Task<List<HtmlFragment>> GetHtmlExtractedFragmentsAsync(SyndicationItem feed)
        {
            if (feed.Links != null && feed.Links.Count != 0)
            {
                var content = await ExtractAsync(feed.Links[0].Uri);

                return content;
            }
            return [];
        }

        private async Task<List<HtmlFragment>> ExtractAsync(Uri hRef)
        {
            logger.LogInformation("Page scrapping {hRef}", hRef);

            var htmlFragments = new List<HtmlFragment>();
            var config = new CrawlConfiguration
            {
                CrawlTimeoutSeconds = 100,
                MaxConcurrentThreads = 10,
                MaxPagesToCrawl = 4,
                MinCrawlDelayPerDomainMilliSeconds = 3000
            };


            var crawler = new PoliteWebCrawler(config);

            crawler.PageCrawlCompleted += (sender, pageCrawlEvent) =>
            {
                if (pageCrawlEvent.CrawledPage.HttpResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var fragments = ExtractElementsText(pageCrawlEvent.CrawledPage.AngleSharpHtmlDocument);
                    htmlFragments.AddRange(fragments);
                }
            };
            await crawler.CrawlAsync(hRef);            
            return htmlFragments;
        }

        private static List<HtmlFragment> ExtractElementsText(IHtmlDocument htmlDocument)
        {
            var fragments = new List<HtmlFragment>();
            var mainTags = htmlDocument.All.Where(p => p.LocalName == "main" && p.ClassList.Contains("wa-container"));

            foreach (var mainTag in mainTags)
            {
                if (mainTag != null)
                {
                    var tags = mainTag.QuerySelectorAll("h1, h2, h3, h4, h5, h6, p");
                    if (tags != null && tags.Length != 0)
                    {
                        foreach (var tag in tags)
                        {
                            var content = tag.TextContent.Trim();
                            if(!string.IsNullOrWhiteSpace(content))
                            {
                                var links = new List<string>();                                
                                var anchors = tag.QuerySelectorAll("a");
                                if (anchors != null && anchors.Length != 0)
                                {
                                    foreach (var anchor in anchors)
                                    {
                                        if (anchor != null && anchor.HasAttribute("href"))
                                        {
                                            var href = anchor.GetAttribute("href");
                                            links.Add(href);                                            
                                        }
                                    }
                                }
                                fragments.Add(new HtmlFragment(content, links));
                            }
                        }
                    }
                }
            }
            return fragments;
        }
    }

    public record HtmlFragment(string Content, List<string> Links);
}