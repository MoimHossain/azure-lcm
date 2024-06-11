

using AzLcm.Shared.Storage;

namespace AzLcm.Shared.ServiceHealth
{
    public class ServiceHealthConfigReader(BlobContentReader blobContentReader)
    {
        public async Task<AzResourceGraphQueryConfig?> GetResourceGraphQueryConfig(
            CancellationToken stoppingToken)
        {
            return await blobContentReader
                .ReadFromJsonAsync<AzResourceGraphQueryConfig>(
                BlobContentReader.ConfigBlobs.ServiceHealthConfig, stoppingToken);
        }

        public async Task<string?> GetKustoQueryAsync(CancellationToken stoppingToken)
        {
            return await blobContentReader.ReadBlobContentAsync(
                BlobContentReader.ConfigBlobs.ServiceHealthQuery, stoppingToken);
        }
    }

    public record AzResourceGraphQueryConfig(string[] Subscriptions, string Uri);
}
