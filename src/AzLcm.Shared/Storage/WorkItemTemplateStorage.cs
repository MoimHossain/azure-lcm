



namespace AzLcm.Shared.Storage
{
    public class WorkItemTemplateStorage(ConfigurationStorage ConfigurationStorage)
    {
        public async Task<WorkItemTemplate?> GetFeedWorkItemTemplateAsync(CancellationToken stoppingToken)
        {
            var templates = await ConfigurationStorage.GetWorkItemTemplatesAsync(stoppingToken);

            return templates.FeedWorkItemTemplate;
        }

        public async Task<WorkItemTemplate?> GetPolicyWorkItemTemplateAsync(CancellationToken stoppingToken)
        {
            var templates = await ConfigurationStorage.GetWorkItemTemplatesAsync(stoppingToken);
            return templates.PolicyWorkItemTemplate;
        }

        public async Task<WorkItemTemplate?> GetServiceHealthWorkItemTemplateAsync(CancellationToken stoppingToken)
        {
            var templates = await ConfigurationStorage.GetWorkItemTemplatesAsync(stoppingToken);
            return templates.ServiceHealthWorkItemTemplate;
        }

        public async Task<AreaPathMapConfig?> GetAreaPathMapConfigAsync(CancellationToken stoppingToken)
        {
            return await ConfigurationStorage.LoadConfigAsync(stoppingToken);
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