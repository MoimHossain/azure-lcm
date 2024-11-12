


using AzLcm.Shared.Storage;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AzLcm.Shared.Cognition.Models
{
    public enum UpdateKind
    {
        Retired,
        Deprecated,
        Deprecation,
        Unsupported,
        Announcement,
        GenerallyAvailable,
        Preview,
        Unknown
    }

    public class Verdict
    {
        public List<string>? AzureServiceNames { get; set; }
        public UpdateKind UpdateKind { get; set; }
        public bool Actionable { get; set; }
        public bool AnnouncementRequired { get; set; }
        public bool ActionableViaAzurePolicy { get; set; }
        public string? MitigationInstructionMarkdown { get; set; }
        public static Verdict? FromJson(
            string rawContent, ILogger logger, JsonSerializerOptions jsonSerializerOptions)
        {
            try
            {
                rawContent = $"{rawContent}".Trim();
                // also cover text starts with ```json and ends with ```
                var startIndex = rawContent.IndexOf("{");
                var endIndex = rawContent.LastIndexOf("}");
                if (startIndex > -1 && endIndex > -1 && startIndex < endIndex)
                {
                    rawContent = rawContent.Substring(startIndex, endIndex - startIndex + 1);
                }
                return JsonSerializer.Deserialize<Verdict>(rawContent, jsonSerializerOptions);
            }
            catch (Exception e)
            {
                logger.LogError(e, message: $"rawContent: {rawContent}");
            }
            return null;
        }
    }
}
