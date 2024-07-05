

namespace AzLcm.Shared.Storage
{
    public class WorkItemTemplateStorage(BlobContentReader blobContentReader)
    {
        public async Task<WorkItemTemplate?> GetFeedWorkItemTemplateAsync(CancellationToken stoppingToken)
        {
            return await blobContentReader
                .ReadFromJsonAsync<WorkItemTemplate>(
                BlobContentReader.ConfigBlobs.FeedWorkItemTemplate, stoppingToken);
        }

        public async Task<WorkItemTemplate?> GetPolicyWorkItemTemplateAsync(CancellationToken stoppingToken)
        {
            return await blobContentReader
                .ReadFromJsonAsync<WorkItemTemplate>(
                BlobContentReader.ConfigBlobs.PolicyWorkItemTemplate, stoppingToken);
        }

        public async Task<WorkItemTemplate?> GetServiceHealthWorkItemTemplateAsync(CancellationToken stoppingToken)
        {
            return await blobContentReader
                .ReadFromJsonAsync<WorkItemTemplate>(
                BlobContentReader.ConfigBlobs.ServiceHealthWorkItemTemplate, stoppingToken);
        }

        public async Task<AreaPathMapConfig?> GetAreaPathMapConfigAsync(CancellationToken stoppingToken)
        {
            return await blobContentReader
                .ReadFromJsonAsync<AreaPathMapConfig>(
                BlobContentReader.ConfigBlobs.AreaPathRouteTemplate, stoppingToken);
        }
    }

    public class WorkItemTemplate
    {
        public string? ProjectId { get; set; }
        public string? Type { get; set; }
        public List<PatchFragment>? Fields { get; set; }
    }

    public class PatchFragment
    {
        public string? Op { get; set; }
        public string? Path { get; set; }
        public string? From { get; set; }
        public string? Value { get; set; }
    }
}