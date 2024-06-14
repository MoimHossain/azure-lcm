namespace AzLcm.Shared.AzureUpdates.Model
{
    public record AzFeedItem(string Title, DateTime PublishedDate, string[] Tags, string HtmlBody, string UpdateBody, string link);

    public static class AzFeedItemExtensions
    {
        private static char[] SEPCHARS = ['/']; 
        public static bool Validate(this AzFeedItem feedItem)
        {
            if(feedItem != null)
            {
                return !string.IsNullOrWhiteSpace(feedItem.Title)
                    && feedItem.PublishedDate != DateTime.MinValue
                    && feedItem.Tags != null && feedItem.Tags.Length > 0
                    && !string.IsNullOrWhiteSpace(feedItem.HtmlBody)
                    && !string.IsNullOrWhiteSpace(feedItem.UpdateBody)
                    && !string.IsNullOrWhiteSpace(feedItem.link);
            }
            return false;
        }

        public static string? GetID(this AzFeedItem feedItem)
        {
            if (feedItem != null && !string.IsNullOrWhiteSpace(feedItem.link))
            {
                var segments = feedItem.link.Split(SEPCHARS, StringSplitOptions.RemoveEmptyEntries);
                if(segments != null && segments.Length > 0)
                {
                    return segments[^1];
                }
            }
            return string.Empty;
        }
    }
}
