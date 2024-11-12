

using AngleSharp.Dom;
using System.Globalization;

namespace AzLcm.Shared.PageScrapping
{
    public static class HtmlExtensions
    {
        public static string SafeReadHref(this IElement? element)
        {
            if (element != null)
            {
                return element.GetAttribute("href") ?? string.Empty;
            }
            return string.Empty;
        }

        public static string SafeReadInnerText(this IElement? element)
        {
            if (element != null)
            {
                return $"{element.TextContent}".Trim();
            }
            return string.Empty;
        }

        public static string SafeReadInnerHtml(this IElement? element)
        {
            if (element != null)
            {
                return $"{element.InnerHtml}".Trim();
            }
            return string.Empty;
        }

        public static DateTime SafeReadDate(this IElement? element, CultureInfo enUSCulture)
        {
            if (element != null)
            {
                var publishedDate = element.SafeReadInnerText();
                publishedDate = publishedDate.Replace("Published date:", "").Trim();
                if (DateTime.TryParse(publishedDate, enUSCulture, out var parsedDate))
                {
                    return parsedDate;
                }
            }
            return DateTime.Now;
        }
    }
}
