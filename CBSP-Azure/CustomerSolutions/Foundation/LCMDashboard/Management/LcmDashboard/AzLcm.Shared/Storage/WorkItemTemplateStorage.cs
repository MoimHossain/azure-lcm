

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

    public class WorkItemTempates
    {
        public WorkItemTemplate? FeedWorkItemTemplate { get; set; }
        public WorkItemTemplate? PolicyWorkItemTemplate { get; set; }
        public WorkItemTemplate? ServiceHealthWorkItemTemplate { get; set; }
    }
    public class WorkItemTemplate
    {
        public string? ProjectId { get; set; }
        public string? Type { get; set; }
        public List<PatchFragment>? Fields { get; set; }

        public static WorkItemTemplate DEFAULT = new WorkItemTemplate
        {
            ProjectId = "1853c648-0f7d-4f1e-80ab-fd1ecd333520",
            Type = "Product Backlog Item",
            Fields = 
            [
                new PatchFragment { Op = "add", Path = "/fields/System.Title", Value = "{Feed.Title}" },
                new PatchFragment { Op = "add", Path = "/fields/Custom.Resource", Value = "{Classification.ServiceName}" },
                new PatchFragment { Op = "add", Path = "/fields/Custom.PublishDate", Value = "{Feed.PublishDate}" },
                new PatchFragment { Op = "add", Path = "/fields/Custom.Reference", Value = "{Feed.Link}" },
                new PatchFragment { Op = "add", Path = "/fields/System.Description", Value = "<p>{Feed.Summary}</p><hr/><p>{Feed.Content.Text}</p>" },
                new PatchFragment { Op = "add", Path = "/fields/System.Tags", Value = "{Classification.Tags}" },
                new PatchFragment { Op = "add", Path = "/fields/System.AreaPath", Value = "Platform\\Engineering\\Cloud\\Azure-Updates" }
            ]
        };
    }

    public class PatchFragment
    {
        public string? Op { get; set; }
        public string? Path { get; set; }
        public string? From { get; set; }
        public string? Value { get; set; }
    }
}