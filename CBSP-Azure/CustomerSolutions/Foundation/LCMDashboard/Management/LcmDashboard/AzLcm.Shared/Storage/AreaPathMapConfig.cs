﻿

using System.Text.Json;
using System.Text.Json.Serialization;

namespace AzLcm.Shared.Storage
{
    public class GeneralConfig
    {
        [property: JsonPropertyName("delayMilliseconds")] public int DelayMilliseconds { get; set; } = 1000 * 60 * 5;

        [property: JsonPropertyName("processServiceHealth")] public bool ProcessServiceHealth { get; set; } = true;
        [property: JsonPropertyName("processPolicy")] public bool ProcessPolicy { get; set; } = true;
        [property: JsonPropertyName("processFeed")] public bool ProcessFeed { get; set; } = true;

        public GeneralConfig() { }
    }

    public class AreaPathServiceMap
    {
        [property: JsonPropertyName("Services")] public List<string> Services { get; set; } = new List<string>();
        [property: JsonPropertyName("RouteToAreaPath")] public string RouteToAreaPath { get; set; } = string.Empty;

        public AreaPathServiceMap(List<string> services, string routeToAreaPath)
        {
            Services = services;
            RouteToAreaPath = routeToAreaPath;
        }

        public AreaPathServiceMap() { }
    }

    public class AreaPathServiceMapConfig
    {
        [property: JsonPropertyName("Map")] public List<AreaPathServiceMap> Map { get; set; } = new List<AreaPathServiceMap>();
        [property: JsonPropertyName("DefaultAreaPath")] public string DefaultAreaPath { get; set; } = string.Empty;
        [property: JsonPropertyName("IgnoreWhenNoMatchFound")] public bool IgnoreWhenNoMatchFound { get; set; }

        public AreaPathServiceMapConfig(List<AreaPathServiceMap> map, string defaultAreaPath, bool ignoreWhenNoMatchFound)
        {
            Map = map;
            DefaultAreaPath = defaultAreaPath;
            IgnoreWhenNoMatchFound = ignoreWhenNoMatchFound;
        }

        public AreaPathServiceMapConfig() { }
    }


    public class AreaPathMapConfig
    {
        [property: JsonPropertyName("ServiceHealthMap")] public AreaPathServiceMapConfig ServiceHealthMap { get; set; } = new AreaPathServiceMapConfig();
        [property: JsonPropertyName("PolicyMap")] public AreaPathServiceMapConfig PolicyMap { get; set; } = new AreaPathServiceMapConfig();
        [property: JsonPropertyName("AzureUpdatesMap")] public AreaPathServiceMapConfig AzureUpdatesMap { get; set; } = new AreaPathServiceMapConfig();

        public AreaPathMapConfig(AreaPathServiceMapConfig serviceHealthMap, AreaPathServiceMapConfig policyMap, AreaPathServiceMapConfig azureUpdatesMap)
        {
            ServiceHealthMap = serviceHealthMap;
            PolicyMap = policyMap;
            AzureUpdatesMap = azureUpdatesMap;
        }

        public AreaPathMapConfig() { }
    }




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
