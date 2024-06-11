

namespace AzLcm.Shared.Storage
{
    public class PromptTemplateStorage(BlobContentReader blobContentReader)
    {
        public async Task<string> GetFeedPromptAsync(CancellationToken stoppingToken)
        {
            var prompt = await blobContentReader.ReadBlobContentAsync(BlobContentReader.ConfigBlobs.FeedPromptTemplate, stoppingToken);

            return prompt;
        }
    }
}
