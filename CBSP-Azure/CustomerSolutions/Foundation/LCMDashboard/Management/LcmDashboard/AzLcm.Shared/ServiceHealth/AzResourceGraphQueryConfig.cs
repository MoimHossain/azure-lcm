
using AzLcm.Shared.Storage;
using System.Text.Json.Serialization;

namespace AzLcm.Shared.ServiceHealth
{
    public class ServiceHealthConfigReader(ConfigurationStorage ConfigurationStorage )
    {
        public async Task<ServiceHealthConfig?> GetResourceGraphQueryConfig(
            CancellationToken stoppingToken)
        {
            return await ConfigurationStorage.LoadServiceHealthConfigAsync(stoppingToken);
        }
    }

    //public record AzResourceGraphQueryConfig(string[] Subscriptions, string Uri);

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
