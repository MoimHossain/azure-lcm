

using System.Reflection;
using System.Text.Json;

namespace AzLcm.Shared.Storage
{
    public class WorkItemTemplateStorage(JsonSerializerOptions jsonSerializerOptions)
    {
        private async Task<string> GetTemplateTextAsync(CancellationToken stoppingToken)
        {
            var resourceName = $"{typeof(WorkItemTemplateStorage).Namespace}.WorkItemTemplate.json";
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            if (stream != null)
            {
                using var reader = new StreamReader(stream);                
                string fileContents = await reader.ReadToEndAsync(stoppingToken);
                return fileContents;
            }
            throw new InvalidOperationException($"Resource {resourceName} not found");
        }

        public async Task<WorkItemTemplate?> GetWorkItemTemplateAsync(CancellationToken stoppingToken)
        {
            var templateText = await GetTemplateTextAsync(stoppingToken);
            var template = JsonSerializer.Deserialize<WorkItemTemplate>(templateText, jsonSerializerOptions);
            return template;
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