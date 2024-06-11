

using System.Net.Http.Json;

namespace AzLcm.Shared.ServiceHealth
{
    public class ServiceHealthConfigReader(DaemonConfig daemonConfig, IHttpClientFactory httpClientFactory)
    {
        public async Task<AzResourceGraphQueryConfig> GetResourceGraphQueryConfig(CancellationToken stoppingToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(daemonConfig.ServiceHealthConfigUri, nameof(daemonConfig.ServiceHealthConfigUri));

            var httpClient = httpClientFactory.CreateClient();
            var azGraphConfig = await httpClient.GetFromJsonAsync<AzResourceGraphQueryConfig>(daemonConfig.ServiceHealthConfigUri, stoppingToken);
            if (azGraphConfig == null)
            {
                throw new InvalidOperationException($"Failed to get azGraphConfig from {daemonConfig.FeedPromptTemplateUri}");
            }
            return azGraphConfig;
        }

        public async Task<string?> GetKustoQueryAsync(CancellationToken stoppingToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(daemonConfig.ServiceHealthKustoQuery, nameof(daemonConfig.ServiceHealthKustoQuery));

            var httpClient = httpClientFactory.CreateClient();
            var queryText = await httpClient.GetStringAsync(daemonConfig.ServiceHealthKustoQuery, stoppingToken);
            return queryText;
        }
    }

    public record AzResourceGraphQueryConfig(string[] Subscriptions, string Uri);
}
