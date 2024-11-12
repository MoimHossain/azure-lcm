

using AzLcm.Shared.Storage.EmbeddedResources;

namespace AzLcm.Shared.Storage
{
    public class PromptTemplateStorage(EmbeddedResourceReader embeddedContentReader)
    {
        public async Task<string> GetFeedPromptAsync(CancellationToken stoppingToken)
        {
            var prompt = await embeddedContentReader.ReadEmbeddedResourceAsync(EmbeddedResourceReader.ConfigBlobs.FeedPromptTemplate, stoppingToken);

            return prompt;
        }
    }
}
