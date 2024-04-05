

using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;

namespace AzLcm.Shared.Storage
{
    public class WorkItemTemplateStorage(
        DaemonConfig daemonConfig, 
        IHttpClientFactory httpClientFactory,
        JsonSerializerOptions jsonSerializerOptions)
    {
        private async Task<string> GetTemplateTextAsync(string resourceName, CancellationToken stoppingToken)
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            if (stream != null)
            {
                using var reader = new StreamReader(stream);                
                string fileContents = await reader.ReadToEndAsync(stoppingToken);
                return fileContents;
            }
            throw new InvalidOperationException($"Resource {resourceName} not found");
        }

        private async Task<WorkItemTemplate?> GetWorkItemTemplateFromEmbeddedResourceAsync(string resourceName, CancellationToken stoppingToken)
        {   
            var templateText = await GetTemplateTextAsync(resourceName, stoppingToken);
            var template = JsonSerializer.Deserialize<WorkItemTemplate>(templateText, jsonSerializerOptions);
            return template;
        }

        private async Task<WorkItemTemplate?> GetWorkItemTemplateFromUriAsync(string uri, CancellationToken stoppingToken)
        {
            var httpClient = httpClientFactory.CreateClient();
            var template = await httpClient.GetFromJsonAsync<WorkItemTemplate>(uri, stoppingToken);
            return template;
        }

        public async Task<WorkItemTemplate?> GetFeedWorkItemTemplateAsync(CancellationToken stoppingToken)
        {
            if(!string.IsNullOrWhiteSpace(daemonConfig.FeedTemplateUri))
            {
                return await GetWorkItemTemplateFromUriAsync(daemonConfig.FeedTemplateUri, stoppingToken);
            }

            var resourceName = $"{typeof(WorkItemTemplateStorage).Namespace}.FeedWorkItemTemplate.json";
            return await GetWorkItemTemplateFromEmbeddedResourceAsync(resourceName, stoppingToken);
        }

        public async Task<WorkItemTemplate?> GetPolicyWorkItemTemplateAsync(CancellationToken stoppingToken)
        {
            if (!string.IsNullOrWhiteSpace(daemonConfig.PolicyTemplateUri))
            {
                return await GetWorkItemTemplateFromUriAsync(daemonConfig.PolicyTemplateUri, stoppingToken);
            }

            var resourceName = $"{typeof(WorkItemTemplateStorage).Namespace}.PolicyWorkItemTemplate.json";
            return await GetWorkItemTemplateFromEmbeddedResourceAsync(resourceName, stoppingToken);
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