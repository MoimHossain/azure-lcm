

using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AzLcm.Shared.Storage
{
    public class BlobContentReader(
        DaemonConfig daemonConfig,
        JsonSerializerOptions jsonSerializerOptions,
        ILogger<BlobContentReader> logger)
    {
        private readonly BlobServiceClient blobServiceClient = new BlobServiceClient(daemonConfig.StorageConnectionString);

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
            var text = await ReadBlobContentAsync(blobName, stoppingToken);

            if(!string.IsNullOrWhiteSpace(text))
            {
                return (TPayloadType?)JsonSerializer.Deserialize<TPayloadType>(text, jsonSerializerOptions);
            }
            throw new InvalidOperationException($"Failed to read blob content from {blobName}");
        }

        public async Task<string> ReadBlobContentAsync(string blobName, CancellationToken stoppingToken)
        {   
            ArgumentNullException.ThrowIfNull(blobName, nameof(blobName));
            
            var containerName = daemonConfig.StorageConfigContainer;
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            try
            {
                var response = await blobClient.DownloadAsync(stoppingToken);
                using var reader = new StreamReader(response.Value.Content);
                return await reader.ReadToEndAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to read blob content from {BlobName}", blobName);
                throw;
            }
        }
    }
}
