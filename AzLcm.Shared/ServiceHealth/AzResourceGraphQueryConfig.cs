

using AzLcm.Shared.Storage;
using AzLcm.Shared.Storage.EmbeddedResources;

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
}
