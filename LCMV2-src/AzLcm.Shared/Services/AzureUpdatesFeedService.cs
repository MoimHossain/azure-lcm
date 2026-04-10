using System.Xml;
using System.ServiceModel.Syndication;
using AzLcm.Shared.Models;

namespace AzLcm.Shared.Services;

public class AzureUpdatesFeedService
{
    private const string AzureUpdatesFeedUrl = "https://www.azure.microsoft.com/en-us/updates/feed/";

    private readonly HttpClient _httpClient;

    public AzureUpdatesFeedService(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
    }

    public async Task<List<AzureUpdateEntry>> FetchUpdatesAsync(CancellationToken cancellationToken = default)
    {
        var entries = new List<AzureUpdateEntry>();

        try
        {
            using var stream = await _httpClient.GetStreamAsync(AzureUpdatesFeedUrl, cancellationToken);
            var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Parse };
            using var xmlReader = XmlReader.Create(stream, settings);
            var feed = SyndicationFeed.Load(xmlReader);

            foreach (var item in feed.Items)
            {
                var entry = new AzureUpdateEntry
                {
                    Id = item.Id ?? Guid.NewGuid().ToString(),
                    Title = item.Title?.Text ?? "Untitled",
                    Summary = item.Summary?.Text ?? string.Empty,
                    Description = GetContentText(item),
                    Link = item.Links.FirstOrDefault()?.Uri?.ToString() ?? string.Empty,
                    PublishedDate = item.PublishDate.UtcDateTime,
                    LastUpdatedDate = item.LastUpdatedTime != DateTimeOffset.MinValue
                        ? item.LastUpdatedTime.UtcDateTime
                        : null,
                    Categories = item.Categories.Select(c => c.Name).ToList(),
                    ChangeType = ClassifyChangeType(item.Title?.Text, item.Summary?.Text),
                    Severity = ClassifySeverity(item.Title?.Text, item.Summary?.Text),
                    IngestedAt = DateTime.UtcNow
                };

                entries.Add(entry);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error fetching Azure Updates feed: {ex.Message}");
        }

        return entries;
    }

    private static string GetContentText(SyndicationItem item)
    {
        if (item.Content is TextSyndicationContent textContent)
            return textContent.Text;
        return item.Summary?.Text ?? string.Empty;
    }

    private static ChangeType ClassifyChangeType(string? title, string? summary)
    {
        var text = $"{title} {summary}".ToLowerInvariant();

        if (text.Contains("retire") || text.Contains("end of life") || text.Contains("eol"))
            return ChangeType.Retirement;
        if (text.Contains("deprecat"))
            return ChangeType.Deprecation;
        if (text.Contains("breaking change") || text.Contains("breaking"))
            return ChangeType.BreakingChange;
        if (text.Contains("security") || text.Contains("vulnerability") || text.Contains("cve"))
            return ChangeType.SecurityAdvisory;
        if (text.Contains("preview"))
            return ChangeType.Preview;
        if (text.Contains("generally available") || text.Contains("general availability") || text.Contains(" ga"))
            return ChangeType.GeneralAvailability;
        if (text.Contains("migration") || text.Contains("migrate"))
            return ChangeType.MigrationRequired;
        if (text.Contains("price") || text.Contains("cost") || text.Contains("pricing"))
            return ChangeType.PriceChange;
        if (text.Contains("region") && text.Contains("available"))
            return ChangeType.RegionExpansion;
        if (text.Contains("new feature") || text.Contains("now available") || text.Contains("announcing"))
            return ChangeType.NewFeature;

        return ChangeType.Unknown;
    }

    private static Severity ClassifySeverity(string? title, string? summary)
    {
        var text = $"{title} {summary}".ToLowerInvariant();

        if (text.Contains("retire") || text.Contains("eol") || text.Contains("end of life")
            || text.Contains("security") || text.Contains("critical"))
            return Severity.Critical;
        if (text.Contains("deprecat") || text.Contains("breaking") || text.Contains("migration required"))
            return Severity.High;
        if (text.Contains("preview") || text.Contains("update"))
            return Severity.Medium;
        if (text.Contains("new feature") || text.Contains("generally available"))
            return Severity.Low;

        return Severity.Informational;
    }
}
