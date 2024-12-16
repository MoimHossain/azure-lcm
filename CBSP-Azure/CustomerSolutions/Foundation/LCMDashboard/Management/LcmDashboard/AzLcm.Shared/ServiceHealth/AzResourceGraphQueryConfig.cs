using AzLcm.Shared.Storage.EmbeddedResources;
using System.Text.Json.Serialization;

namespace AzLcm.Shared.ServiceHealth
{
    public class ServiceHealthConfigReader(EmbeddedResourceReader embeddedResourceReader)
    {
        public async Task<AzResourceGraphQueryConfig?> GetResourceGraphQueryConfig(
            CancellationToken stoppingToken)
        {
            return await embeddedResourceReader
                .ReadFromJsonAsync<AzResourceGraphQueryConfig>(
                EmbeddedResourceReader.ConfigBlobs.ServiceHealthConfig, stoppingToken);
        }

        public async Task<string?> GetKustoQueryAsync(CancellationToken stoppingToken)
        {
            return await embeddedResourceReader.ReadEmbeddedResourceAsync(
                EmbeddedResourceReader.ConfigBlobs.ServiceHealthQuery, stoppingToken);
        }
    }

    public record AzResourceGraphQueryConfig(string[] Subscriptions, string Uri);

    public class ServiceHealthConfig
    {
        [JsonPropertyName("subscriptions")]
        public string[]? Subscriptions { get; set; }

        [JsonPropertyName("uri")]
        public string? Uri { get; set; }

        [JsonPropertyName("kustoQuery")]
        public string? KustoQuery { get; set; }
    }
}
