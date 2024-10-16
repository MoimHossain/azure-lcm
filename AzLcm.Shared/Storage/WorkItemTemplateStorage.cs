

using AzLcm.Shared.Storage.EmbeddedResources;

namespace AzLcm.Shared.Storage
{
    public class WorkItemTemplateStorage(EmbeddedResourceReader embeddedResourceReader)
    {
        public async Task<WorkItemTemplate?> GetFeedWorkItemTemplateAsync(CancellationToken stoppingToken)
        {
            return await embeddedResourceReader
                .ReadFromJsonAsync<WorkItemTemplate>(
                EmbeddedResourceReader.ConfigBlobs.FeedWorkItemTemplate, stoppingToken);
        }

        public async Task<WorkItemTemplate?> GetPolicyWorkItemTemplateAsync(CancellationToken stoppingToken)
        {
            return await embeddedResourceReader
                .ReadFromJsonAsync<WorkItemTemplate>(
                EmbeddedResourceReader.ConfigBlobs.PolicyWorkItemTemplate, stoppingToken);
        }

        public async Task<WorkItemTemplate?> GetServiceHealthWorkItemTemplateAsync(CancellationToken stoppingToken)
        {
            return await embeddedResourceReader
                .ReadFromJsonAsync<WorkItemTemplate>(
                EmbeddedResourceReader.ConfigBlobs.ServiceHealthWorkItemTemplate, stoppingToken);
        }

        public async Task<AreaPathMapConfig?> GetAreaPathMapConfigAsync(CancellationToken stoppingToken)
        {
            return await embeddedResourceReader
                .ReadFromJsonAsync<AreaPathMapConfig>(
                EmbeddedResourceReader.ConfigBlobs.AreaPathRouteTemplate, stoppingToken);
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