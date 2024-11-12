

using Microsoft.Extensions.Logging;
using Serilog.Core;
using System.Text.Json;

namespace AzLcm.Shared.Storage.EmbeddedResources
{
    public class EmbeddedResourceReader(
        ILogger<EmbeddedResourceReader> logger,
        JsonSerializerOptions jsonSerializerOptions)
    {
        public static class ConfigBlobs
        {
            public const string FeedPromptTemplate = "FeedPromptTemplate.txt";
            public const string FeedWorkItemTemplate = "FeedWorkItemTemplate.json";
            public const string PolicyWorkItemTemplate = "PolicyWorkItemTemplate.json";
            public const string ServiceHealthConfig = "service-health-config.json";
            public const string ServiceHealthQuery = "service-health-query.txt";
            public const string ServiceHealthWorkItemTemplate = "Service-health-workitem-template.json";
            public const string AreaPathRouteTemplate = "AreaPathRouteConfig.json";
        }

        public async Task<TPayloadType?> ReadFromJsonAsync<TPayloadType>(string blobName, CancellationToken stoppingToken)
        {
            var text = await ReadEmbeddedResourceAsync(blobName, stoppingToken);

            if (!string.IsNullOrWhiteSpace(text))
            {
                return (TPayloadType?)JsonSerializer.Deserialize<TPayloadType>(text, jsonSerializerOptions);
            }
            throw new InvalidOperationException($"Failed to read blob content from {blobName}");
        }

        public Task<string> ReadEmbeddedResourceAsync(string resourceName, CancellationToken cancellationToken)
        {
            logger.LogInformation("Reading embedded resource {ResourceName}", resourceName);
            var assembly = typeof(EmbeddedResourceReader).Assembly;
            using var stream = assembly.GetManifestResourceStream(typeof(EmbeddedResourceReader), resourceName) 
                ?? throw new InvalidOperationException($"Resource {resourceName} not found.");
            using var reader = new StreamReader(stream);
            return reader.ReadToEndAsync(cancellationToken);
        }
    }
}
