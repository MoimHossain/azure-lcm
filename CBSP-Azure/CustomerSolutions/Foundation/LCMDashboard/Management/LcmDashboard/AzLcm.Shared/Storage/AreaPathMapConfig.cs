using System.Text.Json;
using System.Text.Json.Serialization;

namespace AzLcm.Shared.Storage
{
    public record AreaPathServiceMap(
        [property: JsonPropertyName("Services")] IReadOnlyList<string> Services,
        [property: JsonPropertyName("RouteToAreaPath")] string RouteToAreaPath
    );

    public record AreaPathServiceMapConfig(
        [property: JsonPropertyName("Map")] IReadOnlyList<AreaPathServiceMap> Map,        
        [property: JsonPropertyName("DefaultAreaPath")] string DefaultAreaPath,
        [property: JsonPropertyName("IgnoreWhenNoMatchFound")] bool IgnoreWhenNoMatchFound
    );

    public record AreaPathMapConfig(
        [property: JsonPropertyName("ServiceHealthMap")] AreaPathServiceMapConfig ServiceHealthMap,
        [property: JsonPropertyName("PolicyMap")] AreaPathServiceMapConfig PolicyMap,
        [property: JsonPropertyName("AzureUpdatesMap")] AreaPathServiceMapConfig AzureUpdatesMap
    );

    public class AreaPathMapResponse
    {
        public string? AreaPath { get; set; }

        public static AreaPathMapResponse? FromJson(string rawContent, JsonSerializerOptions jsonSerializerOptions)
        {
            rawContent = $"{rawContent}".Trim();
            // also cover text starts with ```json and ends with ```
            var startIndex = rawContent.IndexOf("{");
            var endIndex = rawContent.LastIndexOf("}");
            if (startIndex > -1 && endIndex > -1 && startIndex < endIndex)
            {
                rawContent = rawContent.Substring(startIndex, endIndex - startIndex + 1);
            }
            return JsonSerializer.Deserialize<AreaPathMapResponse>(rawContent, jsonSerializerOptions);
        }
    }
}
